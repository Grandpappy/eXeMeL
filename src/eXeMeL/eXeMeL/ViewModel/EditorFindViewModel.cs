using eXeMeL.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using eXeMeL.Utilities;


namespace eXeMeL.ViewModel
{
  public class EditorFindViewModel : ObservableObject
  {
    #region Properties


    public TextDocument Document { get; set; }

    private string _currentFindValue;
    private int _matchCount;
    private MatchCollection _matches;
    private int? _currentMatchIndex;
    private bool _useRegex;



    public string SearchText
    {
      get { return this._currentFindValue; }
      set
      {
        if (!string.IsNullOrEmpty(this.SearchText) && string.IsNullOrEmpty(value))
        {
          NavigateToNoMatch();
        }

        SetProperty(ref this._currentFindValue, value);
        OnPropertyChanged(nameof(HasSearchText));
        (FindNextCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (FindPreviousCommand as RelayCommand)?.NotifyCanExecuteChanged();
        this.Matches = null;

        if (value != null && value.Length == 0)
          ClearSearchState();
        else
          _ = PerformFindNextSearchAsync();
      }
    }



    public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

    public bool UseRegex
    {
      get => _useRegex;
      set
      {
        if (SetProperty(ref _useRegex, value))
        {
          this.Matches = null; // Force re-search with new mode
          if (HasSearchText)
            _ = PerformFindNextSearchAsync();
        }
      }
    }

    private string _matchStatusText;
    public string MatchStatusText
    {
      get => _matchStatusText;
      private set => SetProperty(ref _matchStatusText, value);
    }



    public int MatchCount
    {
      get { return this._matchCount; }
      set { SetProperty(ref this._matchCount, value); }
    }



    private MatchCollection Matches
    {
      get { return this._matches; }
      set
      {
        SetProperty(ref this._matches, value, nameof(Matches));

        if (this.Matches == null || this.Matches.Count == 0)
        {
          this.MatchCount = 0;
        }
        else
        {
          this.MatchCount = this.Matches.Count;
        }

        this.CurrentMatchIndex = 0;
      }
    }



    public int? CurrentMatchIndex
    {
      get { return this._currentMatchIndex; }
      set
      {
        if (this.Matches == null || this.Matches.Count == 0)
        {
          SetProperty(ref this._currentMatchIndex, null);
        }
        else
        if (value >= 0 && value < this.Matches.Count)
        {
          SetProperty(ref this._currentMatchIndex, value);
        }
        else
        if (value < 0)
        {
          SetProperty(ref this._currentMatchIndex, 0);
        }
        else
        {
          SetProperty(ref this._currentMatchIndex, this.Matches.Count - 1);
        }
      }
    }



    public int CurrentMatchPosition
    {
      get
      {
        if (this.CurrentMatchIndex.HasValue)
        {
          return this.CurrentMatchIndex.Value + 1;
        }
        else
        {
          return 0;
        }
      }
    }



    private Match CurrentMatch
    {
      get
      {
        if (!this.CurrentMatchIndex.HasValue)
          return null;

        return this.Matches[this.CurrentMatchIndex.Value];
      }
    }



    public ICommand FindNextCommand { get; private set; }
    public ICommand FindPreviousCommand { get; private set; }
    public ICommand CancelSearchCommand { get; private set; }

    #endregion


    #region Construction


    public EditorFindViewModel()
    {
      this.FindNextCommand = new RelayCommand(FindNextCommand_Execute, FindNextCommand_CanExecute);
      this.FindPreviousCommand = new RelayCommand(FindPreviousCommand_Execute, FindPreviousCommand_CanExecute);
      this.CancelSearchCommand = new RelayCommand(CancelSearchCommand_Execute);

      WeakReferenceMessenger.Default.Register<SetSearchTextMessage>(this, (r, m) => HandleSetFindTextMessage(m));

      this.SearchText = string.Empty;
    }

    #endregion


    #region Message Handling


    private void HandleSetFindTextMessage(SetSearchTextMessage message)
    {
      this.SearchText = message.SearchText;
    }


    #endregion


    #region Searching


    private bool FindNextCommand_CanExecute()
    {
      return IsSearchTextValidForSearching();
    }



    private async void FindNextCommand_Execute()
    {
      await PerformFindNextSearchAsync();
    }



    private async Task PerformFindNextSearchAsync()
    {
      if (IsSearchNeeded())
      {
        await PerformSearchAsync();

        if (this.MatchCount == 0)
        {
          NavigateToNoMatch();
        }
        else
        {
          NavigateToFirstMatch();
        }
      }
      else
      {
        NavigateToNextMatch();
      }
    }



    private bool FindPreviousCommand_CanExecute()
    {
      return IsSearchTextValidForSearching();
    }



    private async void FindPreviousCommand_Execute()
    {
      await PerformFindPreviousSearch();
    }



    private async Task PerformFindPreviousSearch()
    {
      if (IsSearchNeeded())
      {
        if (this.MatchCount == 0)
        {
          NavigateToNoMatch();
          return;
        }

        await PerformSearchAsync();
        NavigateToLastMatch();
      }
      else
      {
        NavigateToPreviousMatch();
      }
    }



    private async void AutoFindTimer_Tick(object sender, EventArgs e)
    {
      if (IsSearchTextValidForAutomaticSearching())
        return;

      await PerformFindNextSearchAsync();
      //this.AutoFindTimer.Stop();
    }



    private bool IsSearchTextValidForAutomaticSearching()
    {
      return this.SearchText.Length <= 2;
    }



    private bool IsSearchNeeded()
    {
      return this.Matches == null;
    }



    private async Task PerformSearchAsync()
    {
      if (this.SearchText.Length == 0)
        return;

      var text = this.Document.Text;
      var pattern = UseRegex ? this.SearchText : Regex.Escape(this.SearchText);

      await Task.Run(() =>
      {
        try
        {
          this.Matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
        }
        catch (System.ArgumentException)
        {
          // Invalid regex pattern — clear matches
          this.Matches = null;
        }
      });
    }



    private bool IsSearchTextValidForSearching()
    {
      if (!string.IsNullOrEmpty(this.SearchText))
      {
        return true;
      }
      else
      {
        return false;
      }
    }


    #endregion


    #region Navigation


    private void NavigateToFirstMatch()
    {
      this.CurrentMatchIndex = 0;

      NavigateToCurrentMatch();
    }



    private void NavigateToLastMatch()
    {
      this.CurrentMatchIndex = this.Matches.Count - 1;

      NavigateToCurrentMatch();
    }



    private void NavigateToNextMatch()
    {
      this.CurrentMatchIndex = (this.CurrentMatchIndex + 1) % this.Matches.Count;

      NavigateToCurrentMatch();
    }



    private void NavigateToPreviousMatch()
    {
      var newIndex = (this.CurrentMatchIndex - 1) % this.Matches.Count;
      if (newIndex < 0)
      {
        newIndex = this.Matches.Count + newIndex;
      }

      this.CurrentMatchIndex = newIndex;

      NavigateToCurrentMatch();
    }



    private void NavigateToCurrentMatch()
    {
      if (this.CurrentMatch == null)
        return;

      SendNavigationMessageForCurrentMatch();
    }



    private void NavigateToNoMatch()
    {
      SendNavigationMessageForNoMatch();
    }



    private void CancelSearchCommand_Execute()
    {
      this.SearchText = string.Empty;
    }



    private void SendNavigationMessageForCurrentMatch()
    {
      WeakReferenceMessenger.Default.Send(new SelectTextInEditorMessage(this.CurrentMatch.Index, this.CurrentMatch.Length));
      MatchStatusText = $"{this.CurrentMatchPosition} of {this.MatchCount} matches";
      WeakReferenceMessenger.Default.Send(new DisplayToolInformationMessage(MatchStatusText));
    }



    private void SendNavigationMessageForNoMatch()
    {
      WeakReferenceMessenger.Default.Send(new UnselectTextInEditorMessage());
      MatchStatusText = $"No results";
      WeakReferenceMessenger.Default.Send(new DisplayToolInformationMessage($"Unable to find \"{this.SearchText}\""));
    }



    /// <summary>Clears search state without moving focus (used when text is emptied by typing)</summary>
    private void ClearSearchState()
    {
      MatchStatusText = null;
      WeakReferenceMessenger.Default.Send(new UnselectTextInEditorMessage());
      WeakReferenceMessenger.Default.Send(new DisplayToolInformationMessage(string.Empty));
    }

    /// <summary>Cancels search AND returns focus to editor (used by Escape key)</summary>
    private void CancelSearch()
    {
      MatchStatusText = null;
      WeakReferenceMessenger.Default.Send(new SetKeyboardFocusToEditor());
      WeakReferenceMessenger.Default.Send(new DisplayToolInformationMessage(string.Empty));
    }


    #endregion
  }
}
