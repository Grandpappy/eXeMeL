using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace eXeMeL.View
{
  public partial class ColorPickerControl : UserControl
  {
    private static readonly string[] PresetColors = new[]
    {
      "#0078D4", "#0099BC", "#00B294", "#498205",
      "#107C10", "#7A7574", "#5D5A58", "#4C4A48",
      "#8764B8", "#881798", "#C239B3", "#E3008C",
      "#EA005E", "#D13438", "#DA3B01", "#EF6950",
      "#CA5010", "#FF8C00", "#F7630C", "#FFB900"
    };

    public static readonly DependencyProperty SelectedColorProperty =
      DependencyProperty.Register(nameof(SelectedColor), typeof(string), typeof(ColorPickerControl),
        new FrameworkPropertyMetadata("#3366CC", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public string SelectedColor
    {
      get => (string)GetValue(SelectedColorProperty);
      set => SetValue(SelectedColorProperty, value);
    }

    public ColorPickerControl()
    {
      InitializeComponent();
      BuildSwatches();
      UpdatePreview();
    }

    private void BuildSwatches()
    {
      foreach (var hex in PresetColors)
      {
        var swatch = new Border
        {
          Width = 22,
          Height = 22,
          Margin = new Thickness(2),
          CornerRadius = new CornerRadius(3),
          Cursor = Cursors.Hand,
          Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)),
          BorderThickness = new Thickness(2),
          BorderBrush = Brushes.Transparent,
          Tag = hex
        };
        swatch.MouseLeftButtonDown += Swatch_Click;
        SwatchPanel.Children.Add(swatch);
      }
    }

    private void Swatch_Click(object sender, MouseButtonEventArgs e)
    {
      if (sender is Border border && border.Tag is string hex)
      {
        SelectedColor = hex;
      }
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is ColorPickerControl picker)
      {
        picker.UpdatePreview();
        picker.UpdateSwatchHighlights();
      }
    }

    private void UpdatePreview()
    {
      try
      {
        var color = (Color)ColorConverter.ConvertFromString(SelectedColor);
        ColorPreview.Background = new SolidColorBrush(color);
      }
      catch
      {
        ColorPreview.Background = Brushes.Transparent;
      }
    }

    private void UpdateSwatchHighlights()
    {
      var selected = SelectedColor?.ToUpperInvariant();
      foreach (var child in SwatchPanel.Children)
      {
        if (child is Border swatch && swatch.Tag is string hex)
        {
          swatch.BorderBrush = hex.ToUpperInvariant() == selected
            ? Brushes.White
            : Brushes.Transparent;
        }
      }
    }
  }
}
