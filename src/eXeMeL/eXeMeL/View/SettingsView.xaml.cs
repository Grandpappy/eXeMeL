using eXeMeL.Model;
using eXeMeL.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;

namespace eXeMeL.View
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : UserControl
  {
    public SettingsView()
    {
      InitializeComponent();
      this.AddHandler(System.Windows.UIElement.PreviewMouseWheelEvent, new System.Windows.Input.MouseWheelEventHandler(ComboBox_PreviewMouseWheel), true);
      this.Loaded += SettingsView_Loaded;
    }

    private void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
      var settings = DataContext as Settings;
      if (settings == null) return;

      InitializeEditorBackgroundUI(settings);

      // Watch for property changes that affect editor background UI
      settings.PropertyChanged += Settings_PropertyChanged;

      // Push picker color changes to the setting when unlinked
      var dpd = DependencyPropertyDescriptor.FromProperty(
        ColorPickerPopup.SelectedColorProperty, typeof(ColorPickerPopup));
      dpd?.AddValueChanged(EditorBackgroundColorPicker, OnEditorBackgroundPickerColorChanged);
    }

    private void InitializeEditorBackgroundUI(Settings settings)
    {
      var isLinked = string.IsNullOrEmpty(settings.EditorBackgroundColor);
      EditorBackgroundColorPicker.IsEnabled = !isLinked;
      EditorBackgroundLinkIcon.Symbol = isLinked
        ? Wpf.Ui.Controls.SymbolRegular.Link24
        : Wpf.Ui.Controls.SymbolRegular.LinkDismiss24;
      EditorBackgroundSubtext.Text = isLinked ? "Follows theme" : "Custom color";

      EditorBackgroundColorPicker.SelectedColor = isLinked
        ? ApplicationThemeManager.GetCurrentDerivedEditorColor(settings)
        : settings.EditorBackgroundColor;

      UpdateEditorTintControlsVisibility();
    }

    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var settings = DataContext as Settings;
      if (settings == null) return;

      if (e.PropertyName is nameof(Settings.ApplicationTheme)
          or nameof(Settings.ChromeTintColor)
          or nameof(Settings.EditorTintIntensity))
      {
        UpdateEditorTintControlsVisibility();

        // If linked, update the picker preview to reflect the new derived color
        if (string.IsNullOrEmpty(settings.EditorBackgroundColor))
          EditorBackgroundColorPicker.SelectedColor = ApplicationThemeManager.GetCurrentDerivedEditorColor(settings);
      }
    }

    private void EditorBackgroundLinkButton_Click(object sender, RoutedEventArgs e)
    {
      var settings = DataContext as Settings;
      if (settings == null) return;

      if (string.IsNullOrEmpty(settings.EditorBackgroundColor))
      {
        // Unlink: populate with current derived color
        var derivedColor = ApplicationThemeManager.GetCurrentDerivedEditorColor(settings);
        settings.EditorBackgroundColor = derivedColor;
        EditorBackgroundColorPicker.SelectedColor = derivedColor;
        EditorBackgroundColorPicker.IsEnabled = true;
        EditorBackgroundLinkIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.LinkDismiss24;
        EditorBackgroundSubtext.Text = "Custom color";
      }
      else
      {
        // Relink: clear custom color
        settings.EditorBackgroundColor = null;
        EditorBackgroundColorPicker.SelectedColor = ApplicationThemeManager.GetCurrentDerivedEditorColor(settings);
        EditorBackgroundColorPicker.IsEnabled = false;
        EditorBackgroundLinkIcon.Symbol = Wpf.Ui.Controls.SymbolRegular.Link24;
        EditorBackgroundSubtext.Text = "Follows theme";
      }

      UpdateEditorTintControlsVisibility();
    }

    private void OnEditorBackgroundPickerColorChanged(object sender, EventArgs e)
    {
      var settings = DataContext as Settings;
      if (settings == null) return;

      // Only push to settings when unlinked (picker is enabled)
      if (!string.IsNullOrEmpty(settings.EditorBackgroundColor))
        settings.EditorBackgroundColor = EditorBackgroundColorPicker.SelectedColor;
    }

    private void UpdateEditorTintControlsVisibility()
    {
      // No conditional controls to manage — Editor Opacity is always visible
    }

    private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
      // Prevent scroll wheel from changing ComboBox value unless it's open
      if (e.OriginalSource is System.Windows.DependencyObject source)
      {
        var combo = FindParent<System.Windows.Controls.ComboBox>(source);
        if (combo != null && !combo.IsDropDownOpen)
        {
          e.Handled = true;
        }
      }
    }

    private static T FindParent<T>(System.Windows.DependencyObject child) where T : System.Windows.DependencyObject
    {
      var current = child;
      while (current != null)
      {
        // Use VisualTreeHelper for Visual elements, LogicalTreeHelper for non-visuals (like Run)
        System.Windows.DependencyObject parent;
        if (current is System.Windows.Media.Visual)
          parent = System.Windows.Media.VisualTreeHelper.GetParent(current);
        else
          parent = System.Windows.LogicalTreeHelper.GetParent(current);

        if (parent is T found) return found;
        current = parent;
      }
      return null;
    }



    private void ResetFontSizeButton_Click(object sender, RoutedEventArgs e)
    {
      (this.DataContext as Settings).EditorFontSize = Settings.DefaultEditorFontSize;
    }

    private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
      var button = sender as Wpf.Ui.Controls.Button;
      if (button != null) button.Content = "Checking...";

      var hasUpdate = await App.CheckForUpdatesAsync(silent: false);

      if (hasUpdate)
      {
        var result = System.Windows.MessageBox.Show(
          $"Version {App.LatestUpdate.TargetFullRelease.Version} is available. Update and restart now?",
          "Update Available",
          System.Windows.MessageBoxButton.YesNo,
          System.Windows.MessageBoxImage.Information);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
          if (button != null) button.Content = "Downloading...";
          await App.ApplyUpdateAsync();
        }
      }
      else
      {
        System.Windows.MessageBox.Show("You're running the latest version.",
          "No Updates", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
      }

      if (button != null) button.Content = "Check for Updates";
    }

    private void ResetToDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
      var result = System.Windows.MessageBox.Show(
        "Reset all settings to their default values?",
        "Reset Settings",
        System.Windows.MessageBoxButton.YesNo,
        System.Windows.MessageBoxImage.Question);

      if (result == System.Windows.MessageBoxResult.Yes)
      {
        var settings = this.DataContext as Settings;
        settings?.ResetToDefaults();
        if (settings != null)
          InitializeEditorBackgroundUI(settings);
      }
    }



    public string CurrentVersion
    {
      get
      {
        var v = ApplicationVersionControl.GetPublishedVersion();
        return $"{v.Major}.{v.Minor}.{v.Build}";
      }
    }
  }



  public class EnumerationExtension : MarkupExtension
  {
    private Type _enumType;


    public EnumerationExtension(Type enumType)
    {
      if (enumType == null)
        throw new ArgumentNullException("enumType");

      EnumType = enumType;
    }

    public Type EnumType
    {
      get { return _enumType; }
      private set
      {
        if (_enumType == value)
          return;

        var enumType = Nullable.GetUnderlyingType(value) ?? value;

        if (enumType.IsEnum == false)
          throw new ArgumentException("Type must be an Enum.");

        _enumType = value;
      }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      var enumValues = Enum.GetValues(EnumType);

      return (
        from object enumValue in enumValues
        where DisplayInSettings(enumValue) 
        select new EnumerationMember
        {
          Value = enumValue,
          Description = GetDescription(enumValue)
        }).ToArray();
    }

    private string GetDescription(object enumValue)
    {
      var descriptionAttribute = EnumType
        .GetField(enumValue.ToString())
        .GetCustomAttributes(typeof(DescriptionAttribute), false)
        .FirstOrDefault() as DescriptionAttribute;


      return descriptionAttribute != null
        ? descriptionAttribute.Description
        : enumValue.ToString();
    }

    private bool DisplayInSettings(object enumValue)
    {
      var attribute = EnumType
        .GetField(enumValue.ToString())
        .GetCustomAttributes(typeof(DoNotDisplayInSettingsAttribute), false)
        .FirstOrDefault() as DoNotDisplayInSettingsAttribute;

      if (attribute == null)
        return true;
      else
        return false;
    }

    public class EnumerationMember
    {
      public string Description { get; set; }
      public object Value { get; set; }
    }
  }

}
