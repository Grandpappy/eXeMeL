# Add JSON Support to eXeMeL

## Context

eXeMeL is an XML editor that developers use all day for viewing, formatting, and analyzing XML content. The user wants to extend it to also handle JSON — auto-detecting the format, cleaning/formatting it, providing syntax highlighting, and offering a JSON Tree viewer comparable to the XPath utility. A clickable XML/JSON toggle in the status bar allows manual override when auto-detection is wrong.

No new NuGet packages needed — `System.Text.Json` ships with .NET 9.

---

## Phase 1: Foundation — DocumentContentType + Auto-Detection

**New files:**
- `Model/DocumentContentType.cs` — enum: `Xml`, `Json`
- `Model/ContentTypeDetector.cs` — static class with `Detect(string content)` method

**Detection logic:** Trim whitespace, check first character: `<` = XML, `{` or `[` = JSON. Fall back to XML for unknown.

**Modified files:**
- `ViewModel/EditorViewModel.cs`:
  - Add `DocumentContentType ContentType` observable property
  - In `CleanXmlIfPossibleAsync` (rename to `CleanContentAsync`): detect content type first, then run XML or JSON cleaner chain accordingly
  - In `OpenFileAsync`: detect content type from file content
  - Send a new `ContentTypeChangedMessage` when content type changes
- `Messages/ContentTypeChangedMessage.cs` — new message carrying the DocumentContentType

---

## Phase 2: JSON Cleaners (parallel with Phase 3 & 4)

**New files in `ViewModel/JsonCleaners/`:**
- `JsonCleanerBase.cs` + `JsonCleanerContext.cs` — mirror XmlCleanerBase pattern. Context has `TextToClean` (string), `FormattedJson` (string), `ErrorMessage` (string)
- `JsonUrlEncodingCleaner.cs` — `WebUtility.UrlDecode()` (same as XML)
- `JsonTrimCleaner.cs` — `.Trim()` (same as XML)
- `JsonEscapeCleaner.cs` — unescape `\"` → `"`, `\\n` → newline, `\\t` → tab (for JSON pasted from string literals)
- `JsonSurroundingGarbageCleaner.cs` — find first `{` or `[` to matching last `}` or `]`, strip surrounding text
- `JsonFormatCleaner.cs` — parse with `JsonDocument.Parse()`, serialize with `JsonSerializer.Serialize()` with `WriteIndented = true` for pretty-printing. If parse fails, set ErrorMessage.

**Modified files:**
- `ViewModel/EditorViewModel.cs`:
  - Add `List<JsonCleanerBase> JsonCleaners` initialized in constructor
  - Add `CleanJsonAsync(JsonCleanerContext)` method mirroring `CleanXml`
  - `CleanContentAsync` dispatches to XML or JSON cleaner chain based on ContentType

---

## Phase 3: JSON Syntax Highlighting (parallel with Phase 2 & 4)

**New .xshd files in `Assets/SyntaxHighlightingSchemes/`:**
- `JsonBright.xshd` — light theme
- `JsonEarthy.xshd` — light theme (warm)
- `JsonDark.xshd` — dark ethereal theme
- `JsonVSBlue.xshd` — dark VS-style theme
- `JsonSolarizedDark.xshd` — solarized dark theme

**JSON token types and colors (VS Blue dark example):**
| Token | Color | Rule |
|-------|-------|------|
| Property key | `#9CDCFE` (cyan) | `"[^"]*"\s*:` (string followed by colon) |
| String value | `#CE9178` (brown) | `"[^"]*"` (any quoted string not a key) |
| Number | `#B5CEA8` (green) | `-?\d+(\.\d+)?([eE][+-]?\d+)?` |
| Boolean | `#569CD6` (blue) | `\btrue\b\|false\b` |
| Null | `#569CD6` (blue) | `\bnull\b` |
| Braces/brackets | default text | `[{}\[\]]` |
| Colon/comma | default text (dimmed) | `[:,]` |

**Modified files:**
- `eXeMeL.csproj` — add `<EmbeddedResource>` entries for JSON .xshd files with LogicalName
- `Model/SyntaxHighlightingStyleEnum.cs` — add `[AssociatedJsonEmbeddedResource]` attribute to each existing enum value, mapping it to the corresponding JSON .xshd
- `Model/Attributes.cs` — add `AssociatedJsonEmbeddedResourceAttribute` class
- `ViewModel/SyntaxHighlightManager.cs`:
  - Add `DocumentContentType _contentType` field
  - Add `SetContentType(DocumentContentType)` method
  - `GetSyntaxHighlighting()` checks content type: if JSON, load the JSON .xshd; if XML, load the XML .xshd (current behavior)
  - Listen for `ContentTypeChangedMessage` to re-load highlighting

---

## Phase 4: JSON Folding (parallel with Phase 2 & 3)

**New file:**
- `ViewModel/JsonFoldingStrategy.cs` — implements `AbstractFoldingStrategy` (or manual `IFoldingStrategy`). Scans for `{`/`}` and `[`/`]` pairs, respecting string literals. Creates fold regions.

**Modified files:**
- `MainWindow.xaml.cs`:
  - Store both `XmlFoldingStrategy` and `JsonFoldingStrategy`
  - Listen for `ContentTypeChangedMessage`
  - On content type change, swap the active folding strategy and re-fold
  - `UpdateDocumentFoldings()` calls the active strategy

---

## Phase 5: JSON Tree Viewer

**New files:**
- `ViewModel/JsonUtility/JsonUtilityViewModel.cs` — parses JSON string into a tree of JsonNodeViewModels. Has `DocumentText`, `Root`, `IsBusy`, `IsJsonValid` properties. Built on background thread (same pattern as XmlUtilityViewModel).
- `ViewModel/JsonUtility/JsonNodeViewModel.cs` — represents a JSON node. Properties: `Name` (key or index), `Value` (for primitives), `NodeType` (Object/Array/String/Number/Boolean/Null), `Children` (list), `IsExpanded`. For objects: children are key-value pairs. For arrays: children are indexed items.
- `View/JsonUtilityView.xaml` — TreeView with HierarchicalDataTemplate (same virtualization pattern as XmlUtilityView). Nodes display: `"key": value` for primitives, `"key": { ... }` for objects, `"key": [ ... ]` for arrays. Color-coded by type using theme brushes.
- `View/JsonUtilityView.xaml.cs` — minimal code-behind (click to expand, focus handling)

**Design:**
- Uses `System.Text.Json.JsonDocument` / `JsonElement` for parsing
- Tree nodes created on background thread, Root set triggers UI binding
- No XPath equivalent needed — just navigation and expand/collapse
- Context menu: copy JSON path, copy value, expand all, collapse all

---

## Phase 6: Tab Switching + Status Bar Toggle

**Modified files:**
- `MainWindow.xaml`:
  - Add `JsonTreeTabHeader` border (similar to XPathTabHeader)
  - Add `JsonTreePanel` border containing JsonUtilityView (similar to XPathPanel)
  - In the status bar footer, add a clickable TextBlock showing "XML" or "JSON" with gold accent
- `MainWindow.xaml.cs`:
  - `JsonTreeTabHeader_Click` handler
  - Listen for `ContentTypeChangedMessage` → update tab visibility (show/hide XPath vs JSON Tree), update status bar label, swap folding strategy
  - Status bar click handler: toggles ContentType on EditorViewModel, does NOT re-clean content — only swaps highlighting, folding, and tabs
- `ViewModel/MainViewModel.cs`:
  - Add `JsonUtilityViewModel JsonUtility` property
  - Wire up `ContentTypeChangedMessage` → sync JSON utility document text when switching to JSON Tree tab

**Tab visibility rules:**
| ContentType | Editor tab | XPath tab | JSON Tree tab |
|-------------|-----------|-----------|---------------|
| XML | Visible | Visible | Hidden |
| JSON | Visible | Hidden | Visible |

---

## Phase 7: Content Type in Snapshots

**Modified files:**
- `ViewModel/EditorViewModel.cs` — `DocumentSnapshot` (inner class or wherever snapshots are stored) gains a `ContentType` property. When navigating to a snapshot, restore its ContentType.

---

## Implementation Order

```
Phase 1 (Foundation)
    ├── Phase 2 (JSON Cleaners)      ← parallel
    ├── Phase 3 (Syntax Highlighting) ← parallel
    └── Phase 4 (JSON Folding)        ← parallel
Phase 5 (JSON Tree Viewer)            ← after Phase 1
Phase 6 (Tab Switching + Status Bar)  ← after Phase 1, 3, 4, 5
Phase 7 (Snapshots)                   ← after Phase 1
```

---

## Verification

After each phase, build and run:
```powershell
cd src\eXeMeL
dotnet build eXeMeL\eXeMeL.csproj
dotnet run --project eXeMeL\eXeMeL.csproj
```

**Test scenarios:**
1. Copy XML to clipboard → F5 → should auto-detect XML, clean/format, show XPath tab
2. Copy JSON to clipboard → F5 → should auto-detect JSON, clean/format, show JSON Tree tab
3. Copy JSON string with escapes (`{\"name\":\"test\"}`) → F5 → should clean escapes and pretty-print
4. Open .xml file → should detect XML
5. Open .json file → should detect JSON
6. Click "JSON" in status bar while viewing XML → should switch to JSON highlighting (content unchanged)
7. Click "XML" in status bar while viewing JSON → should switch to XML highlighting
8. F2 to switch to tree tab → XPath for XML, JSON Tree for JSON
9. Create snapshot in JSON mode → switch to XML → snapshot back → should restore JSON mode
10. Run `dotnet test eXeMeL.Tests\eXeMeL.Tests.csproj` — all existing tests pass

**New tests to add:**
- ContentTypeDetector unit tests (XML detection, JSON detection, edge cases)
- JSON cleaner unit tests (each cleaner individually, full pipeline)
- JSON formatting round-trip test
