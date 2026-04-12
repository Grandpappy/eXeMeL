using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YamlDotNet.RepresentationModel;

namespace eXeMeL.ViewModel.YamlUtility
{
  public enum YamlNodeType
  {
    Mapping,
    Sequence,
    Scalar
  }

  public class YamlNodeViewModel : ObservableObject
  {
    private bool _isExpanded = true;

    public string Name { get; }
    public string Value { get; }
    public YamlNodeType NodeType { get; }
    public List<YamlNodeViewModel> Children { get; }
    public bool HasChildren => Children.Count > 0;
    public int ChildCount => Children.Count;

    public bool IsExpanded
    {
      get => _isExpanded;
      set => SetProperty(ref _isExpanded, value);
    }

    public ICommand CollapseAllCommand { get; }
    public ICommand ExpandAllCommand { get; }

    public YamlNodeViewModel(string name, YamlNode node)
    {
      Name = name;
      Children = new List<YamlNodeViewModel>();
      CollapseAllCommand = new RelayCommand(() => SetExpandedRecursive(false));
      ExpandAllCommand = new RelayCommand(() => SetExpandedRecursive(true));

      switch (node)
      {
        case YamlMappingNode mapping:
          NodeType = YamlNodeType.Mapping;
          foreach (var entry in mapping.Children)
          {
            var key = (entry.Key as YamlScalarNode)?.Value ?? entry.Key.ToString();
            Children.Add(new YamlNodeViewModel(key, entry.Value));
          }
          break;

        case YamlSequenceNode sequence:
          NodeType = YamlNodeType.Sequence;
          int index = 0;
          foreach (var item in sequence.Children)
          {
            Children.Add(new YamlNodeViewModel($"[{index}]", item));
            index++;
          }
          break;

        case YamlScalarNode scalar:
          NodeType = YamlNodeType.Scalar;
          Value = scalar.Value;
          break;
      }
    }

    private void SetExpandedRecursive(bool expanded)
    {
      IsExpanded = expanded;
      foreach (var child in Children)
        child.SetExpandedRecursive(expanded);
    }
  }
}
