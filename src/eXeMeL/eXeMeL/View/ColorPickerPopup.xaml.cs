using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace eXeMeL.View
{
  public partial class ColorPickerPopup : UserControl
  {
    private bool _updatingFromSlider;

    public static readonly DependencyProperty SelectedColorProperty =
      DependencyProperty.Register(nameof(SelectedColor), typeof(string), typeof(ColorPickerPopup),
        new FrameworkPropertyMetadata("#D4AA00", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public static readonly DependencyProperty SwatchColorsProperty =
      DependencyProperty.Register(nameof(SwatchColors), typeof(string[]), typeof(ColorPickerPopup),
        new PropertyMetadata(null, OnSwatchColorsChanged));

    public string SelectedColor
    {
      get => (string)GetValue(SelectedColorProperty);
      set => SetValue(SelectedColorProperty, value);
    }

    public string[] SwatchColors
    {
      get => (string[])GetValue(SwatchColorsProperty);
      set => SetValue(SwatchColorsProperty, value);
    }

    public ColorPickerPopup()
    {
      InitializeComponent();
      this.Loaded += (s, e) =>
      {
        BuildSwatches();
        UpdateVisuals();
      };
    }

    private void ColorSwatchButton_Click(object sender, RoutedEventArgs e)
    {
      PickerPopup.IsOpen = !PickerPopup.IsOpen;
      if (PickerPopup.IsOpen)
        UpdateSlidersFromColor();
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is ColorPickerPopup picker)
      {
        picker.UpdateVisuals();
        if (!picker._updatingFromSlider)
          picker.UpdateSlidersFromColor();
      }
    }

    private static void OnSwatchColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is ColorPickerPopup picker)
        picker.BuildSwatches();
    }

    private void BuildSwatches()
    {
      SwatchPanel.Children.Clear();
      var colors = SwatchColors;
      if (colors == null) return;

      foreach (var hex in colors)
      {
        var swatch = new System.Windows.Controls.Border
        {
          Width = 20, Height = 20,
          Margin = new Thickness(1),
          CornerRadius = new CornerRadius(3),
          Cursor = Cursors.Hand,
          Background = CreateFrozenBrush(hex),
          BorderThickness = new Thickness(2),
          BorderBrush = Brushes.Transparent,
          Tag = hex
        };
        swatch.MouseLeftButtonDown += (s, e) =>
        {
          if (s is System.Windows.Controls.Border b && b.Tag is string h)
            SelectedColor = h;
        };
        SwatchPanel.Children.Add(swatch);
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

    private void UpdateVisuals()
    {
      try
      {
        var color = (Color)ColorConverter.ConvertFromString(SelectedColor);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        if (ColorSwatchButton != null) ColorSwatchButton.Background = brush;
        if (PreviewSwatch != null) PreviewSwatch.Background = brush;
        if (HexLabel != null) HexLabel.Text = SelectedColor;
      }
      catch { }

      // Update swatch highlights
      if (SwatchPanel == null) return;
      var selected = SelectedColor?.ToUpperInvariant();
      foreach (var child in SwatchPanel.Children)
      {
        if (child is System.Windows.Controls.Border swatch && swatch.Tag is string hex)
        {
          swatch.BorderBrush = hex.ToUpperInvariant() == selected
            ? Brushes.White : Brushes.Transparent;
        }
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

    private static SolidColorBrush CreateFrozenBrush(string hex)
    {
      var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
      brush.Freeze();
      return brush;
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
      return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }

    private static (double h, double s, double v) ColorToHsv(Color color)
    {
      double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
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
