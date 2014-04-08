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
      get { return _currentFindValue; }
      set 
      {
        if (!string.IsNullOrEmpty(this.SearchText) && string.IsNullOrEmpty(value))
        {
          NavigateToNoMatch();
        }

        this.Set(() => SearchText, ref _currentFindValue, value);
        this.Matches = null;

        PerformFindNextSearchAsync();
      }
    }



    public int MatchCount
    {
      get { return _matchCount; }
      set { this.Set(() => MatchCount, ref _matchCount, value); }
    }
    


    private MatchCollection Matches
    {
      get { return _matches; }
      set 
      { 
        this.Set(() => Matches, ref _matches, value);

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
      get { return _currentMatchIndex; }
      set 
      { 
        if (this.Matches == null || this.Matches.Count == 0)
        {
          this.Set(() => CurrentMatchIndex, ref _currentMatchIndex, null);
        }
        else
        if (value >= 0 && value < this.Matches.Count)
        {
          this.Set(() => CurrentMatchIndex, ref _currentMatchIndex, value);
        }
        else
        if (value < 0)
        {
          this.Set(() => CurrentMatchIndex, ref _currentMatchIndex, 0);
        }
        else
        {
          this.Set(() => CurrentMatchIndex, ref _currentMatchIndex, this.Matches.Count - 1);
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


    #endregion


    #region Construction


    public EditorFindViewModel()
    {
      this.FindNextCommand = new RelayCommand(FindNextCommand_Execute, FindNextCommand_CanExecute);
      this.FindPreviousCommand = new RelayCommand(FindPreviousCommand_Execute, FindPreviousCommand_CanExecute);

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

    

    async private void FindNextCommand_Execute()
    {
      await PerformFindNextSearchAsync();
    }



    async private Task PerformFindNextSearchAsync()
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



    async private void FindPreviousCommand_Execute()
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



    async private void AutoFindTimer_Tick(object sender, EventArgs e)
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



    async private Task PerformSearchAsync()
    {
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



    private void SendNavigationMessageForCurrentMatch()
    {
      this.MessengerInstance.Send<SelectTextInEditorMessage>(new SelectTextInEditorMessage(this.CurrentMatch.Index, this.CurrentMatch.Length));
    }



    private void SendNavigationMessageForNoMatch()
    {
      this.MessengerInstance.Send<UnselectTextInEditorMessage>(new UnselectTextInEditorMessage());
    }


    #endregion
  }
}
