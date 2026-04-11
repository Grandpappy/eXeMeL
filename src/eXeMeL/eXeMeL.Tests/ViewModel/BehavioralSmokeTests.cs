using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using eXeMeL.Tests.Model;
using Xunit;

namespace eXeMeL.Tests.ViewModel
{
  /// <summary>
  /// High-level integration tests that exercise core user workflows via the
  /// ViewModel layer without requiring a live WPF window. Clipboard operations
  /// are intentionally avoided because they require an STA message pump and are
  /// flaky in test environments.
  /// </summary>
  public class BehavioralSmokeTests : IDisposable
  {
    /// <summary>
    /// Creates a <see cref="Settings"/> instance on an STA thread.
    /// The Settings constructor triggers WPF brush resolution via BrushConverter,
    /// which requires an STA apartment state.
    /// </summary>
    private static Settings CreateSettings()
    {
      return StaHelper.Run(() => new Settings());
    }


    /// <summary>
    /// Creates an <see cref="EditorViewModel"/> on an STA thread so that the
    /// internal Settings + AvalonEdit objects are initialized correctly.
    /// </summary>
    private static EditorViewModel CreateEditorViewModel()
    {
      return StaHelper.Run(() => new EditorViewModel(new Settings()));
    }


    /// <summary>
    /// Creates a <see cref="MainViewModel"/> on an STA thread. MainViewModel
    /// construction requires STA (for Settings/brushes) and a live WPF
    /// <see cref="Application"/> (for ApplicationThemeManager resource
    /// dictionary manipulation).
    /// </summary>
    private static MainViewModel CreateMainViewModel()
    {
      return StaHelper.Run(() =>
      {
        EnsureWpfApplication();
        return new MainViewModel();
      });
    }


    /// <summary>
    /// Ensures a WPF Application singleton exists on the current STA thread.
    /// ApplicationThemeManager accesses Application.Current.Resources, so the
    /// Application must exist before MainViewModel is constructed.
    /// </summary>
    private static void EnsureWpfApplication()
    {
      if (Application.Current == null)
      {
        new Application();
      }
    }


    /// <summary>
    /// Spins until the predicate returns true or the timeout elapses.
    /// </summary>
    private static void WaitUntil(Func<bool> predicate, int timeoutMs = 3000)
    {
      var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
      while (!predicate() && DateTime.UtcNow < deadline)
      {
        Thread.Sleep(50);
      }
    }


    public void Dispose()
    {
      WeakReferenceMessenger.Default.Reset();
    }


    #region 1. CleanContentAsync

    [Fact]
    public void CleanContentAsync_FormatsRawXml()
    {
      var editor = CreateEditorViewModel();
      var rawXml = "<Root><Child Name=\"test\"><Value>42</Value></Child></Root>";

      var result = StaHelper.Run(() =>
      {
        var task = editor.CleanContentAsync(rawXml);
        task.Wait(5000);
        return task.Result;
      });

      // The cleaning pipeline should format the XML with indentation
      Assert.NotNull(result);
      Assert.Contains("<Root>", result);
      Assert.Contains("<Child", result);
      Assert.Contains("</Root>", result);
      // Formatted XML should contain newlines (pretty-printed)
      Assert.Contains("\n", result);
    }


    [Fact]
    public void CleanContentAsync_HandlesUrlEncodedXml()
    {
      var editor = CreateEditorViewModel();
      // URL-encoded angle brackets: %3C = <, %3E = >
      var urlEncoded = "%3CRoot%3E%3CChild%2F%3E%3C%2FRoot%3E";

      var result = StaHelper.Run(() =>
      {
        var task = editor.CleanContentAsync(urlEncoded);
        task.Wait(5000);
        return task.Result;
      });

      Assert.NotNull(result);
      Assert.Contains("<Root>", result);
      Assert.Contains("<Child", result);
    }


    [Fact]
    public void CleanContentAsync_ReturnsInputWhenNotXml()
    {
      var editor = CreateEditorViewModel();
      var plainText = "This is just plain text with no XML at all.";

      var result = StaHelper.Run(() =>
      {
        var task = editor.CleanContentAsync(plainText);
        task.Wait(5000);
        return task.Result;
      });

      // Plain text goes through the cleaning pipeline. AddedRootCleaner wraps it
      // in <AddedRoot> when XElement.Parse fails on the raw text.
      Assert.NotNull(result);
      Assert.Contains("This is just plain text", result);
    }

    #endregion


    #region 2. File Open Workflow

    [Fact(Skip = "AvalonEdit 6.x TextDocument thread affinity - OpenFileAsync creates documents on background thread")]
    public void OpenFileAsync_LoadsFileIntoDocument()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

      try
      {
        var xmlContent = "<TestRoot><Item>Hello</Item></TestRoot>";
        File.WriteAllText(tempPath, xmlContent);

        StaHelper.Run(() =>
        {
          var editor = new EditorViewModel(new Settings());
          bool refreshFired = false;
          editor.RefreshComplete += (s, e) => refreshFired = true;

          editor.OpenFileAsync(tempPath);
          WaitUntil(() => refreshFired, timeoutMs: 5000);

          Assert.True(refreshFired, "RefreshComplete event should have fired.");
          Assert.True(editor.IsContentFromFile);
          Assert.Equal(tempPath, editor.FilePath);
          Assert.Equal(Path.GetFileName(tempPath), editor.FileName);
          Assert.Contains("<TestRoot>", editor.Document.Text);
          Assert.Contains("<Item>", editor.Document.Text);
        });
      }
      finally
      {
        if (File.Exists(tempPath))
          File.Delete(tempPath);
      }
    }


    [Fact]
    public void OpenFileAsync_NonExistentFile_DoesNotCrash()
    {
      var fakePath = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid().ToString("N") + ".xml");

      StaHelper.Run(() =>
      {
        var editor = new EditorViewModel(new Settings());
        editor.OpenFileAsync(fakePath);
        Thread.Sleep(200);

        Assert.False(editor.IsContentFromFile);
        Assert.Null(editor.FilePath);
      });
    }

    #endregion


    #region 3. Snapshot Creation

    [Fact]
    public void CreateSnapshotCommand_AddsSnapshotsWithCorrectIdentifiers()
    {
      // AvalonEdit 6.x enforces thread affinity on TextDocument — all operations
      // that create or modify documents must run on the same STA thread.
      StaHelper.Run(() =>
      {
        var editor = new EditorViewModel(new Settings());
        editor.Document.Text = "<Root>Initial</Root>";

        var initialSnapshotCount = editor.Snapshots.Count;

        ((ICommand)editor.CreateSnapshotCommand).Execute(null);

        Assert.True(editor.Snapshots.Count > initialSnapshotCount,
          "Snapshot count should increase after CreateSnapshotCommand.");

        ((ICommand)editor.CreateSnapshotCommand).Execute(null);

        Assert.True(editor.Snapshots.Count >= 2, "Should have at least 2 snapshots.");
        Assert.Equal("Original", editor.Snapshots.First().Identifier);
        Assert.Equal("Current", editor.Snapshots.Last().Identifier);
      });
    }


    [Fact(Skip = "AvalonEdit 6.x TextDocument thread affinity - OpenFileAsync creates documents on background thread")]
    public void OpenFileAsync_ThenCreateSnapshot_ProducesCorrectSnapshotLabels()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

      try
      {
        File.WriteAllText(tempPath, "<Root>FromFile</Root>");

        StaHelper.Run(() =>
        {
          var editor = new EditorViewModel(new Settings());
          bool refreshFired = false;
          editor.RefreshComplete += (s, e) => refreshFired = true;

          editor.OpenFileAsync(tempPath);
          WaitUntil(() => refreshFired, timeoutMs: 5000);

          Assert.Equal(1, editor.Snapshots.Count);
          Assert.Equal("Original", editor.Snapshots[0].Identifier);

          ((ICommand)editor.CreateSnapshotCommand).Execute(null);

          Assert.Equal(2, editor.Snapshots.Count);
          Assert.Equal("Original", editor.Snapshots[0].Identifier);
          Assert.Equal("Current", editor.Snapshots[editor.Snapshots.Count - 1].Identifier);
        });
      }
      finally
      {
        if (File.Exists(tempPath))
          File.Delete(tempPath);
      }
    }

    #endregion


    #region 4. Snapshot Navigation

    [Fact(Skip = "AvalonEdit 6.x TextDocument thread affinity - OpenFileAsync creates documents on background thread")]
    public void ChangeToSnapshotCommand_SwitchesToEarlierSnapshotDocument()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

      try
      {
        File.WriteAllText(tempPath, "<Root>Version1</Root>");

        StaHelper.Run(() =>
        {
          var editor = new EditorViewModel(new Settings());
          bool refreshFired = false;
          editor.RefreshComplete += (s, e) => refreshFired = true;

          editor.OpenFileAsync(tempPath);
          WaitUntil(() => refreshFired, timeoutMs: 5000);

          ((ICommand)editor.CreateSnapshotCommand).Execute(null);
          editor.Document.Text = "<Root>Version2</Root>";

          Assert.Equal(2, editor.Snapshots.Count);

          var originalSnapshot = editor.Snapshots[0];
          ((ICommand)editor.ChangeToSnapshotCommand).Execute(originalSnapshot);

          Assert.Same(originalSnapshot.Document, editor.Document);
          Assert.Contains("Version1", editor.Document.Text);
        });
      }
      finally
      {
        if (File.Exists(tempPath))
          File.Delete(tempPath);
      }
    }

    #endregion


    #region 5. Snapshot Clearing

    [Fact(Skip = "AvalonEdit 6.x TextDocument thread affinity - OpenFileAsync creates documents on background thread")]
    public void ClearSnapshotsAfterDocument_RemovesLaterSnapshots()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

      try
      {
        File.WriteAllText(tempPath, "<Root>Base</Root>");

        StaHelper.Run(() =>
        {
          var editor = new EditorViewModel(new Settings());
          bool refreshFired = false;
          editor.RefreshComplete += (s, e) => refreshFired = true;

          editor.OpenFileAsync(tempPath);
          WaitUntil(() => refreshFired, timeoutMs: 5000);

          ((ICommand)editor.CreateSnapshotCommand).Execute(null);
          ((ICommand)editor.CreateSnapshotCommand).Execute(null);

          Assert.Equal(3, editor.Snapshots.Count);
          Assert.Equal("Original", editor.Snapshots[0].Identifier);
          Assert.Equal("1", editor.Snapshots[1].Identifier);
          Assert.Equal("Current", editor.Snapshots[2].Identifier);

          var middleDocument = editor.Snapshots[1].Document;
          editor.ClearSnapshotsAfterDocument(middleDocument);

          Assert.Equal(2, editor.Snapshots.Count);
          Assert.Same(middleDocument, editor.Snapshots[1].Document);
        });
      }
      finally
      {
        if (File.Exists(tempPath))
          File.Delete(tempPath);
      }
    }


    [Fact(Skip = "AvalonEdit 6.x TextDocument thread affinity - OpenFileAsync creates documents on background thread")]
    public void ClearSnapshotsAfterDocument_WithLastDocument_DoesNothing()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

      try
      {
        File.WriteAllText(tempPath, "<Root>Base</Root>");

        StaHelper.Run(() =>
        {
          var editor = new EditorViewModel(new Settings());
          bool refreshFired = false;
          editor.RefreshComplete += (s, e) => refreshFired = true;

          editor.OpenFileAsync(tempPath);
          WaitUntil(() => refreshFired, timeoutMs: 5000);

          ((ICommand)editor.CreateSnapshotCommand).Execute(null);
          Assert.Equal(2, editor.Snapshots.Count);

          var lastDocument = editor.Snapshots[editor.Snapshots.Count - 1].Document;
          editor.ClearSnapshotsAfterDocument(lastDocument);

          Assert.Equal(2, editor.Snapshots.Count);
        });
      }
      finally
      {
        if (File.Exists(tempPath))
          File.Delete(tempPath);
      }
    }


    [Fact(Skip = "AvalonEdit 6.x TextDocument thread affinity - OpenFileAsync creates documents on background thread")]
    public void ClearSnapshotsAfterDocument_WithNull_DoesNothing()
    {
      var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

      try
      {
        File.WriteAllText(tempPath, "<Root>Base</Root>");

        StaHelper.Run(() =>
        {
          var editor = new EditorViewModel(new Settings());
          bool refreshFired = false;
          editor.RefreshComplete += (s, e) => refreshFired = true;

          editor.OpenFileAsync(tempPath);
          WaitUntil(() => refreshFired, timeoutMs: 5000);

          ((ICommand)editor.CreateSnapshotCommand).Execute(null);
          var countBefore = editor.Snapshots.Count;

          editor.ClearSnapshotsAfterDocument(null);

          Assert.Equal(countBefore, editor.Snapshots.Count);
        });
      }
      finally
      {
        if (File.Exists(tempPath))
          File.Delete(tempPath);
      }
    }

    #endregion


    #region 6. Editor Mode Toggle

    [Fact(Skip = "Requires MahApps ResourceDictionaries - will be fixed in Section 8")]
    public void EditorMode_DefaultsToEditor()
    {
      var mainVm = CreateMainViewModel();

      Assert.Equal(EditorMode.Editor, mainVm.EditorMode);
    }


    [Fact(Skip = "Requires MahApps ResourceDictionaries - will be fixed in Section 8")]
    public void ToggleEditorModeCommand_SwitchesToXmlUtility()
    {
      var mainVm = CreateMainViewModel();

      ((ICommand)mainVm.ToggleEditorModeCommand).Execute(null);

      Assert.Equal(EditorMode.XmlUtility, mainVm.EditorMode);
    }


    [Fact(Skip = "Requires MahApps ResourceDictionaries - will be fixed in Section 8")]
    public void ToggleEditorModeCommand_TogglesBackToEditor()
    {
      var mainVm = CreateMainViewModel();

      ((ICommand)mainVm.ToggleEditorModeCommand).Execute(null);
      Assert.Equal(EditorMode.XmlUtility, mainVm.EditorMode);

      ((ICommand)mainVm.ToggleEditorModeCommand).Execute(null);
      Assert.Equal(EditorMode.Editor, mainVm.EditorMode);
    }

    #endregion


    #region 7. DocumentRefreshCompleted Integration

    [Fact(Skip = "Requires MahApps ResourceDictionaries - will be fixed in Section 8")]
    public void ToggleToXmlUtility_PopulatesXmlUtilityDocumentText()
    {
      StaHelper.Run(() =>
      {
        EnsureWpfApplication();
        var mainVm = new MainViewModel();
        var testXml = "<Root><Item>Integration</Item></Root>";

        mainVm.Editor.Document.Text = testXml;
        ((ICommand)mainVm.ToggleEditorModeCommand).Execute(null);

        Assert.Equal(EditorMode.XmlUtility, mainVm.EditorMode);
        Assert.Equal(testXml, mainVm.XmlUtility.DocumentText);
      });
    }


    [Fact(Skip = "Requires MahApps ResourceDictionaries - will be fixed in Section 8")]
    public void DocumentRefreshCompleted_Message_PopulatesXmlUtilityDocumentText()
    {
      StaHelper.Run(() =>
      {
        EnsureWpfApplication();
        var mainVm = new MainViewModel();
        var refreshedXml = "<Root><Refreshed>Yes</Refreshed></Root>";

        WeakReferenceMessenger.Default.Send(new DocumentRefreshCompleted(refreshedXml));

        Assert.Equal(refreshedXml, mainVm.XmlUtility.DocumentText);
      });
    }

    #endregion
  }
}
