using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace eXeMeL.ViewModel.JsonUtility
{
  public enum JsonNodeType
  {
    Object,
    Array,
    String,
    Number,
    Boolean,
    Null
  }

  public class JsonNodeViewModel : ObservableObject
  {
    private bool _isExpanded = true;

    public string Name { get; }
    public string Value { get; }
    public JsonNodeType NodeType { get; }
    public List<JsonNodeViewModel> Children { get; }
    public bool HasChildren => Children.Count > 0;
    public int ChildCount => Children.Count;

    public bool IsExpanded
    {
      get => _isExpanded;
      set => SetProperty(ref _isExpanded, value);
    }

    public string DisplayText
    {
      get
      {
        if (Name == null)
          return ValueDisplay;
        return NodeType switch
        {
          JsonNodeType.Object => $"\"{Name}\": {{ {ChildCount} }}",
          JsonNodeType.Array => $"\"{Name}\": [ {ChildCount} ]",
          _ => $"\"{Name}\": {ValueDisplay}"
        };
      }
    }

    private string ValueDisplay => NodeType switch
    {
      JsonNodeType.String => $"\"{Value}\"",
      JsonNodeType.Null => "null",
      _ => Value ?? ""
    };

    public ICommand CollapseAllCommand { get; }
    public ICommand ExpandAllCommand { get; }

    public JsonNodeViewModel(string name, JsonElement element)
    {
      Name = name;
      Children = new List<JsonNodeViewModel>();
      CollapseAllCommand = new RelayCommand(() => SetExpandedRecursive(false));
      ExpandAllCommand = new RelayCommand(() => SetExpandedRecursive(true));

      switch (element.ValueKind)
      {
        case JsonValueKind.Object:
          NodeType = JsonNodeType.Object;
          foreach (var prop in element.EnumerateObject())
          {
            Children.Add(new JsonNodeViewModel(prop.Name, prop.Value));
          }
          break;

        case JsonValueKind.Array:
          NodeType = JsonNodeType.Array;
          int index = 0;
          foreach (var item in element.EnumerateArray())
          {
            Children.Add(new JsonNodeViewModel($"[{index}]", item));
            index++;
          }
          break;

        case JsonValueKind.String:
          NodeType = JsonNodeType.String;
          Value = element.GetString();
          break;

        case JsonValueKind.Number:
          NodeType = JsonNodeType.Number;
          Value = element.GetRawText();
          break;

        case JsonValueKind.True:
        case JsonValueKind.False:
          NodeType = JsonNodeType.Boolean;
          Value = element.GetBoolean().ToString().ToLower();
          break;

        case JsonValueKind.Null:
        case JsonValueKind.Undefined:
          NodeType = JsonNodeType.Null;
          Value = "null";
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
