// Markdown preview bridge.
// Communicates with the WPF host (MarkdownUtilityView) via window.chrome.webview.
//
// Inbound (host -> page) message shapes (JSON, sent via PostWebMessageAsJson):
//   { type: 'setContent', html: '<...>' }
//   { type: 'setTheme', theme: 'light' | 'dark' | 'solarized-dark' }
//   { type: 'highlightLine', line: <int> }  // flash a highlight, no scroll
//   { type: 'scrollToLine', line: <int> }   // scroll to line, no persistent highlight
//
// Outbound (page -> host) shapes (sent via chrome.webview.postMessage):
//   { type: 'scroll', line: <int> }
//   { type: 'openExternal', url: '...' }

(function () {
  'use strict';

  const contentEl = document.getElementById('content');
  const bodyEl = document.body;

  // Suppress scroll-event echoing during programmatic scrolls.
  let suppressScrollEvents = 0;
  function withSuppressedScroll(fn) {
    suppressScrollEvents++;
    try { fn(); } finally {
      // Two requestAnimationFrames is enough for browser scroll to settle.
      requestAnimationFrame(() => requestAnimationFrame(() => { suppressScrollEvents--; }));
    }
  }

  // ---------- Inbound messages ----------
  function onHostMessage(e) {
    const msg = e.data;
    if (!msg || typeof msg !== 'object') return;
    switch (msg.type) {
      case 'setContent': setContent(msg.html); break;
      case 'setTheme': setTheme(msg.theme); break;
      case 'highlightLine': highlightLineAt(msg.line); break;
      case 'scrollToLine': scrollToLineAt(msg.line); break;
    }
  }

  function setContent(html) {
    // Preserve scroll position relative to the closest [data-line] element.
    // This survives edits that change line count more gracefully than scrollTop alone.
    const anchor = findScrollAnchor();
    withSuppressedScroll(() => {
      contentEl.innerHTML = html || '';
      bodyEl.classList.toggle('empty', !html);
      if (anchor !== null) restoreScrollAnchor(anchor);
    });
  }

  function setTheme(theme) {
    const valid = ['light', 'dark', 'solarized-dark'];
    document.documentElement.dataset.theme = valid.indexOf(theme) >= 0 ? theme : 'light';
  }

  // Flash a brief highlight on the matching line without scrolling — used for caret click.
  function highlightLineAt(line) {
    if (typeof line !== 'number' || line < 0) return;
    const target = findElementForLine(line);
    if (target) highlightLine(target);
  }

  // Scroll the matching line into view without leaving a persistent highlight —
  // used by linked scroll sync.
  function scrollToLineAt(line) {
    if (typeof line !== 'number' || line < 0) return;
    const target = findElementForLine(line);
    if (!target) return;
    withSuppressedScroll(() => {
      const rect = target.getBoundingClientRect();
      const desiredFromTop = Math.max(0, window.innerHeight * 0.25);
      const delta = rect.top - desiredFromTop;
      window.scrollBy({ top: delta, left: 0, behavior: 'auto' });
    });
  }

  // ---------- Line/anchor helpers ----------
  function getAllLineElements() {
    return contentEl.querySelectorAll('[data-line]');
  }

  function findElementForLine(line) {
    // Best-effort: find the [data-line] whose value is <= line and closest to it.
    let best = null;
    let bestLine = -1;
    for (const el of getAllLineElements()) {
      const n = parseInt(el.dataset.line, 10);
      if (isNaN(n)) continue;
      if (n <= line && n > bestLine) {
        bestLine = n;
        best = el;
      }
    }
    return best;
  }

  function findScrollAnchor() {
    // The line element closest to the top of the viewport, plus its pixel offset.
    let best = null;
    let bestOffset = Number.POSITIVE_INFINITY;
    for (const el of getAllLineElements()) {
      const rect = el.getBoundingClientRect();
      if (rect.bottom < 0) continue; // above viewport
      if (Math.abs(rect.top) < Math.abs(bestOffset)) {
        bestOffset = rect.top;
        best = el;
      }
    }
    if (!best) return null;
    return { line: parseInt(best.dataset.line, 10), offset: bestOffset };
  }

  function restoreScrollAnchor(anchor) {
    if (!anchor) return;
    const target = findElementForLine(anchor.line);
    if (!target) return;
    const rect = target.getBoundingClientRect();
    const delta = rect.top - anchor.offset;
    window.scrollBy({ top: delta, left: 0, behavior: 'auto' });
  }

  // ---------- Active-line highlight ----------
  let activeLineEl = null;
  function highlightLine(el) {
    if (activeLineEl && activeLineEl !== el) {
      activeLineEl.classList.remove('active-line');
    }
    el.classList.add('active-line');
    activeLineEl = el;
    // Auto-clear after a moment so it's a soft cue, not permanent.
    setTimeout(() => {
      if (activeLineEl === el) {
        el.classList.remove('active-line');
        activeLineEl = null;
      }
    }, 1500);
  }

  // ---------- Outbound: scroll position -> host ----------
  let scrollPending = false;
  function onScroll() {
    if (suppressScrollEvents > 0) return;
    if (scrollPending) return;
    scrollPending = true;
    requestAnimationFrame(() => {
      scrollPending = false;
      const anchor = findScrollAnchor();
      if (anchor !== null && !isNaN(anchor.line)) {
        postToHost({ type: 'scroll', line: anchor.line });
      }
    });
  }

  // ---------- Outbound: external link click -> host ----------
  function onClick(e) {
    const a = e.target.closest('a[href]');
    if (!a) return;
    const href = a.getAttribute('href');
    if (!href) return;
    // Allow in-page anchors to behave normally.
    if (href.startsWith('#')) return;
    // Everything else opens in the user's default browser via the host.
    e.preventDefault();
    postToHost({ type: 'openExternal', url: a.href });
  }

  function postToHost(payload) {
    if (window.chrome && window.chrome.webview && window.chrome.webview.postMessage) {
      window.chrome.webview.postMessage(payload);
    }
  }

  // ---------- Wire up ----------
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', onHostMessage);
  }
  window.addEventListener('scroll', onScroll, { passive: true });
  document.addEventListener('click', onClick);

  // Initial state: empty until host sends content.
  bodyEl.classList.add('empty');

  // Tell host we're ready (so it can flush any queued setContent/setTheme/revealLine).
  postToHost({ type: 'ready' });
})();
