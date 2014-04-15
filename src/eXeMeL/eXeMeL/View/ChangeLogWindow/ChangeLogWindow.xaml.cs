using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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



    public ChangeLogWindow()
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
