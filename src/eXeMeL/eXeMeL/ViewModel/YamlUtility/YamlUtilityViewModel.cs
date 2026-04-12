using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using eXeMeL.Model;
using YamlDotNet.RepresentationModel;

namespace eXeMeL.ViewModel.YamlUtility
{
  public class YamlUtilityViewModel : ObservableObject
  {
    public Settings Settings { get; }
    private string _documentText;
    private bool _isYamlValid;
    private YamlNodeViewModel _root;
    private bool _isBusy;

    public YamlUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
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

    public bool IsYamlValid
    {
      get => _isYamlValid;
      set
      {
        SetProperty(ref _isYamlValid, value);
        OnPropertyChanged(nameof(ShowInvalidMessage));
      }
    }

    public bool ShowInvalidMessage => !IsYamlValid && !string.IsNullOrWhiteSpace(DocumentText);

    public bool IsBusy
    {
      get => _isBusy;
      set => SetProperty(ref _isBusy, value);
    }

    public YamlNodeViewModel Root
    {
      get => _root;
      set
      {
        SetProperty(ref _root, value);
        OnPropertyChanged(nameof(RootItems));
      }
    }

    public List<YamlNodeViewModel> RootItems =>
      Root != null ? new List<YamlNodeViewModel> { Root } : new List<YamlNodeViewModel>();

    public void ParseDocumentText()
    {
      if (string.IsNullOrWhiteSpace(DocumentText))
      {
        Root = null;
        IsYamlValid = false;
        return;
      }

      IsBusy = true;

      _ = Task.Run(() =>
      {
        try
        {
          var yaml = new YamlStream();
          using (var reader = new StringReader(DocumentText))
          {
            yaml.Load(reader);
          }

          if (yaml.Documents.Count > 0)
          {
            var rootNode = new YamlNodeViewModel(null, yaml.Documents[0].RootNode);
            Root = rootNode;
            IsYamlValid = true;
          }
          else
          {
            Root = null;
            IsYamlValid = false;
          }
        }
        catch (Exception)
        {
          Root = null;
          IsYamlValid = false;
        }
        finally
        {
          IsBusy = false;
        }
      });
    }
  }
}
