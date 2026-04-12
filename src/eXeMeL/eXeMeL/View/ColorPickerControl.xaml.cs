using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace eXeMeL.View
{
  public partial class ColorPickerControl : UserControl
  {
    // Dark palette: rich deep colors suitable for dark themes
    private static readonly string[] DarkPresets = new[]
    {
      "#1A1A2E", "#16213E", "#0F3460", "#1B1464",
      "#2C003E", "#512B58", "#4A0E4E", "#1C0C27",
      "#0D1B2A", "#1B263B", "#003049", "#023E8A",
      "#0077B6", "#264653", "#2D6A4F", "#1B4332",
      "#3C1642", "#7B2D8E", "#541388", "#0B3D91"
    };

    // Light palette: soft pastel colors suitable for light themes
    private static readonly string[] LightPresets = new[]
    {
      "#E8F0FE", "#D4E4FC", "#BCD4F7", "#A8C7F0",
      "#F3E8FF", "#E8D5F5", "#FDEBED", "#FDE2E4",
      "#E2F0CB", "#D4EDDA", "#D1ECF1", "#CCE5FF",
      "#FFF3CD", "#FEEBC8", "#FFE0B2", "#F8D7DA",
      "#E0E7FF", "#C7D2FE", "#DDD6FE", "#FBCFE8"
    };

    private bool _updatingFromSlider;

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
      BuildSwatches(DarkSwatchPanel, DarkPresets);
      BuildSwatches(LightSwatchPanel, LightPresets);
      UpdatePreview();
      UpdateSlidersFromColor();
    }

    private void BuildSwatches(WrapPanel panel, string[] colors)
    {
      foreach (var hex in colors)
      {
        var swatch = new Border
        {
          Width = 18,
          Height = 18,
          Margin = new Thickness(1),
          CornerRadius = new CornerRadius(3),
          Cursor = Cursors.Hand,
          Background = CreateFrozenBrush(hex),
          BorderThickness = new Thickness(2),
          BorderBrush = Brushes.Transparent,
          Tag = hex
        };
        swatch.MouseLeftButtonDown += Swatch_Click;
        panel.Children.Add(swatch);
      }
    }

    private static SolidColorBrush CreateFrozenBrush(string hex)
    {
      var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
      brush.Freeze();
      return brush;
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
        if (!picker._updatingFromSlider)
          picker.UpdateSlidersFromColor();
      }
    }

    private void HsvSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (HueSlider == null || SatSlider == null || ValSlider == null) return;

      _updatingFromSlider = true;
      var color = HsvToColor(HueSlider.Value, SatSlider.Value / 100.0, ValSlider.Value / 100.0);
      SelectedColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
      _updatingFromSlider = false;
    }

    private void UpdatePreview()
    {
      try
      {
        var color = (Color)ColorConverter.ConvertFromString(SelectedColor);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        if (ColorPreview != null)
          ColorPreview.Background = brush;
      }
      catch
      {
        if (ColorPreview != null)
          ColorPreview.Background = Brushes.Transparent;
      }
    }

    private void UpdateSlidersFromColor()
    {
      if (HueSlider == null) return;

      try
      {
        var color = (Color)ColorConverter.ConvertFromString(SelectedColor);
        var (h, s, v) = ColorToHsv(color);
        HueSlider.Value = h;
        SatSlider.Value = s * 100;
        ValSlider.Value = v * 100;
      }
      catch { }
    }

    private void UpdateSwatchHighlights()
    {
      var selected = SelectedColor?.ToUpperInvariant();
      UpdatePanelHighlights(DarkSwatchPanel, selected);
      UpdatePanelHighlights(LightSwatchPanel, selected);
    }

    private static void UpdatePanelHighlights(WrapPanel panel, string selected)
    {
      if (panel == null) return;
      foreach (var child in panel.Children)
      {
        if (child is Border swatch && swatch.Tag is string hex)
        {
          swatch.BorderBrush = hex.ToUpperInvariant() == selected
            ? Brushes.White
            : Brushes.Transparent;
        }
      }
    }

    private static Color HsvToColor(double h, double s, double v)
    {
      h = h % 360;
      var c = v * s;
      var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
      var m = v - c;

      double r, g, b;
      if (h < 60) { r = c; g = x; b = 0; }
      else if (h < 120) { r = x; g = c; b = 0; }
      else if (h < 180) { r = 0; g = c; b = x; }
      else if (h < 240) { r = 0; g = x; b = c; }
      else if (h < 300) { r = x; g = 0; b = c; }
      else { r = c; g = 0; b = x; }

      return Color.FromRgb(
        (byte)((r + m) * 255),
        (byte)((g + m) * 255),
        (byte)((b + m) * 255));
    }

    private static (double h, double s, double v) ColorToHsv(Color color)
    {
      double r = color.R / 255.0;
      double g = color.G / 255.0;
      double b = color.B / 255.0;

      var max = Math.Max(r, Math.Max(g, b));
      var min = Math.Min(r, Math.Min(g, b));
      var delta = max - min;

      double h = 0;
      if (delta > 0)
      {
        if (max == r) h = 60 * (((g - b) / delta) % 6);
        else if (max == g) h = 60 * (((b - r) / delta) + 2);
        else h = 60 * (((r - g) / delta) + 4);
      }
      if (h < 0) h += 360;

      var s = max > 0 ? delta / max : 0;
      return (h, s, max);
    }
  }
}
