using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;
using eXeMeL.Model;
using ICSharpCode.AvalonEdit.Utils;
using MahApps.Metro.Controls;

namespace eXeMeL.View.ChangeLog
{
  /// <summary>
  /// Interaction logic for ChangeLogWindow.xaml
  /// </summary>
  public partial class ChangeLogWindow : MetroWindow
  {
    public ObservableCollection<ChangeLogEntry> Entries { get; private set; }


    public ChangeLogWindow(ApplicationTheme theme)
    {
      this.Entries = new ObservableCollection<ChangeLogEntry>();

      if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
      {
        this.Entries.Add(new ChangeLogHeader("Second Entry"));
        this.Entries.Add(new ChangeLogContent("The second entry goes on top of the first entry, since the item on top is the newest"));
        this.Entries.Add(new ChangeLogHeader("First Entry"));
        this.Entries.Add(new ChangeLogContent("The first entry is short and sweet"));
        this.Entries.Add(new ChangeLogContent("The second line of the first entry is longer.  This should hopefully show what the line looks like when it has to wrap due to a lot of text."));
      }

      this.Loaded += ChangeLogWindow_Loaded;
      InitializeComponent();

      SetApplicationThemeBasedOnSettings(theme);
    }



    private void ChangeLogWindow_Loaded(object sender, RoutedEventArgs e)
    {
      LoadChangeLoadFromFile();
    }



    private void LoadChangeLoadFromFile()
    {
      using (var stream = this.GetType().Assembly.GetManifestResourceStream("eXeMeL.Assets.ChangeLog.txt"))
      {
        var changeLogContent = FileReader.ReadFileContent(stream, Encoding.ASCII);
        var lines = changeLogContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
          AddChangeLogEntry(line);
        }
      }
    }



    private void AddChangeLogEntry(string line)
    {
      if (line.StartsWith("*"))
      {
        this.Entries.Add(new ChangeLogHeader(line.Substring(1)));
      }
      else
      {
        this.Entries.Add(new ChangeLogContent(line));
      }
    }



    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }


    #region Theme Control

    // TODO: This should be extracted out so it can be shared by this window and the main application

    private void SetApplicationThemeBasedOnSettings(ApplicationTheme theme)
    {
      Debug.Assert(Application.Current.Resources.MergedDictionaries.Count <= 6, "There are more resource dictionaries than expected.");

      if (this.Resources.MergedDictionaries.Count == 6)
      {
        this.Resources.MergedDictionaries.RemoveAt(5);
      }

      var dict = new ResourceDictionary() { Source = new Uri(GetApplicationThemeResource(theme), UriKind.RelativeOrAbsolute) };
      this.Resources.MergedDictionaries.Add(dict);

      this.GlowBrush.Color = (Color)this.FindResource("WindowGlowColor");
    }



    private string GetApplicationThemeResource(ApplicationTheme theme)
    {
      switch (theme)
      {
        case ApplicationTheme.Light:
          return @"pack://application:,,,/Resources/ThemeColors.xaml";

        case ApplicationTheme.Dark:
          return @"pack://application:,,,/Resources/DarkThemeColors.xaml";

        case ApplicationTheme.SolarizedDark:
          return @"pack://application:,,,/Resources/SolarizedDarkThemeColors.xaml";

        default:
          return @"pack://application:,,,/Resources/ThemeColors.xaml";
      }
    }

    #endregion
  }



  public class ChangeLogEntry
  {
    public string Text { get; private set; }

    public ChangeLogEntry(string text)
    {
      this.Text = text;
    }
  }



  public class ChangeLogHeader : ChangeLogEntry
  {
    public ChangeLogHeader(string text)
      : base(text)
    {
    }
  }



  public class ChangeLogContent : ChangeLogEntry
  {
    public ChangeLogContent(string text)
      : base(text)
    {
    }
  }
}
