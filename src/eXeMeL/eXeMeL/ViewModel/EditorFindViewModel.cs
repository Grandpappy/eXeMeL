using eXeMeL.Messages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
  public class EditorFindViewModel : ViewModelBase
  {
    #region Properties


    public TextDocument Document { get; set; }

    private string _currentFindValue;
    private int _matchCount;
    private MatchCollection _matches;
    private int? _currentMatchIndex;



    public string SearchText
    {
      get { return this._currentFindValue; }
      set 
      {
        if (!string.IsNullOrEmpty(this.SearchText) && string.IsNullOrEmpty(value))
        {
          NavigateToNoMatch();
        }

        Set(() => this.SearchText, ref this._currentFindValue, value);
        this.Matches = null;

        if (value != null && value.Length == 0)
          CancelSearch();
        else
          PerformFindNextSearchAsync();
      }
    }



    public int MatchCount
    {
      get { return this._matchCount; }
      set { Set(() => this.MatchCount, ref this._matchCount, value); }
    }
    


    private MatchCollection Matches
    {
      get { return this._matches; }
      set 
      { 
        Set(() => this.Matches, ref this._matches, value);

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
          Set(() => this.CurrentMatchIndex, ref this._currentMatchIndex, null);
        }
        else
        if (value >= 0 && value < this.Matches.Count)
        {
          Set(() => this.CurrentMatchIndex, ref this._currentMatchIndex, value);
        }
        else
        if (value < 0)
        {
          Set(() => this.CurrentMatchIndex, ref this._currentMatchIndex, 0);
        }
        else
        {
          Set(() => this.CurrentMatchIndex, ref this._currentMatchIndex, this.Matches.Count - 1);
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

      this.MessengerInstance.Register<SetSearchTextMessage>(this, HandleSetFindTextMessage);

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

      //this.AutoFindTimer.Stop();

      var text = this.Document.Text;

      await Task.Run(() =>
        this.Matches = Regex.Matches(text, Regex.Escape(this.SearchText), RegexOptions.IgnoreCase)
      );
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
      this.MessengerInstance.Send(new SelectTextInEditorMessage(this.CurrentMatch.Index, this.CurrentMatch.Length));
      this.MessengerInstance.Send(new DisplayToolInformationMessage($"{this.CurrentMatchPosition} of {this.MatchCount} matches"));
    }



    private void SendNavigationMessageForNoMatch()
    {
      this.MessengerInstance.Send(new UnselectTextInEditorMessage());
      this.MessengerInstance.Send(new DisplayToolInformationMessage($"Unable to find \"{this.SearchText}\""));
    }



    private void CancelSearch()
    {
      this.MessengerInstance.Send(new SetKeyboardFocusToEditor());
      this.MessengerInstance.Send(new DisplayToolInformationMessage(string.Empty));
    }


    #endregion
  }
}
