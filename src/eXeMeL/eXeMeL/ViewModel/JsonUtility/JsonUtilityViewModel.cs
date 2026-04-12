using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using eXeMeL.Messages;
using eXeMeL.Model;

namespace eXeMeL.ViewModel.JsonUtility
{
  public class JsonUtilityViewModel : ObservableObject
  {
    public Settings Settings { get; }
    private string _documentText;
    private bool _isJsonValid;
    private JsonNodeViewModel _root;
    private bool _isBusy;

    public JsonUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
      WeakReferenceMessenger.Default.Register<DocumentRefreshCompleted>(this, (r, m) => HandleDocumentRefreshMessage(m));
    }

    private void HandleDocumentRefreshMessage(DocumentRefreshCompleted message)
    {
      // Only parse if we're in JSON mode — will be set by MainViewModel
    }

    public string DocumentText
    {
      get => _documentText;
      set
      {
        SetProperty(ref _documentText, value);
        ParseDocumentText();
      }
    }

    public bool IsJsonValid
    {
      get => _isJsonValid;
      set
      {
        SetProperty(ref _isJsonValid, value);
        OnPropertyChanged(nameof(ShowInvalidMessage));
      }
    }

    public bool ShowInvalidMessage => !IsJsonValid && !string.IsNullOrWhiteSpace(DocumentText);

    public bool IsBusy
    {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
    }

    public JsonNodeViewModel Root
    {
      get => _root;
      set
      {
        SetProperty(ref _root, value);
        OnPropertyChanged(nameof(RootItems));
      }
    }

    public List<JsonNodeViewModel> RootItems =>
      Root != null ? new List<JsonNodeViewModel> { Root } : new List<JsonNodeViewModel>();

    public void ParseDocumentText()
    {
      if (string.IsNullOrWhiteSpace(DocumentText))
      {
        Root = null;
        IsJsonValid = false;
        return;
      }

      IsBusy = true;

      _ = Task.Run(() =>
      {
        try
        {
          using var doc = JsonDocument.Parse(DocumentText);
          var rootNode = new JsonNodeViewModel(null, doc.RootElement);

          Root = rootNode;
          IsJsonValid = true;
        }
        catch (Exception)
        {
          Root = null;
          IsJsonValid = false;
        }
        finally
        {
          IsBusy = false;
        }
      });
    }
  }
}
