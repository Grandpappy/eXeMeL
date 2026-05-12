using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.MarkdownUtility;

namespace eXeMeL.View
{
  public partial class MarkdownUtilityView : UserControl, INotifyPropertyChanged
  {
    public MarkdownUtilityView()
    {
      InitializeComponent();
      this.DataContextChanged += (s, e) =>
      {
        OnPropertyChanged("ViewModel");
        OnPropertyChanged("Settings");
      };

      // Watch for when MdXaml sets/replaces the FlowDocument after rendering
      var dpd = DependencyPropertyDescriptor.FromProperty(
        FlowDocumentScrollViewer.DocumentProperty,
        typeof(FlowDocumentScrollViewer));
      dpd.AddValueChanged(this.MarkdownViewer, (s, e) => ApplyThemeToDocument());

      // Re-theme when application theme changes
      WeakReferenceMessenger.Default.Register<ApplicationThemeUpdatedMessage>(this, (r, m) =>
        Dispatcher.BeginInvoke(new Action(ApplyThemeToDocument)));

      // Re-theme when syntax highlighting style changes (triggers re-load of .xshd colors)
      WeakReferenceMessenger.Default.Register<ContentTypeChangedMessage>(this, (r, m) =>
        Dispatcher.BeginInvoke(new Action(ApplyThemeToDocument)));
    }

    public MarkdownUtilityViewModel ViewModel => this.DataContext as MarkdownUtilityViewModel;
    public Settings Settings => this.ViewModel?.Settings;
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void ApplyThemeToDocument()
    {
      var doc = this.MarkdownViewer.Document;
      if (doc == null) return;

      bool isLight = IsLightTheme();

      // Colors matching MarkdownLight.xshd / MarkdownDark.xshd
      var textBrush = FrozenBrush(isLight ? "#222222" : "#E0E0E0");
      var headingBrush = FrozenBrush(isLight ? "#0000FF" : "#569CD6");
      var codeBrush = FrozenBrush(isLight ? "#A31515" : "#CE9178");
      var linkBrush = FrozenBrush(isLight ? "#0451A5" : "#9CDCFE");
      var blockquoteBrush = FrozenBrush(isLight ? "#008000" : "#6A9955");
      var codeBlockBg = FrozenBrush(isLight ? "#F0F0F0" : "#1E1E1E");
      var inlineCodeBg = FrozenBrush(isLight ? "#E8E8E8" : "#2D2D30");

      // Table colors — explicit for both dark and light to prevent MdXaml defaults
      var tableBorderBrush = FrozenBrush(isLight ? "#D0D0D0" : "#555555");
      var tableHeaderBg = FrozenBrush(isLight ? "#E0E0E0" : "#333333");
      var tableHeaderFg = FrozenBrush(isLight ? "#111111" : "#E0E0E0");
      var tableEvenRowBg = FrozenBrush(isLight ? "#F8F8F8" : "#2A2A2A");
      var tableOddRowBg = FrozenBrush(isLight ? "#FFFFFF" : "#222222");
      var tableCellFg = textBrush;

      // Set document-level defaults
      doc.Foreground = textBrush;
      doc.Background = Brushes.Transparent;
      doc.FontFamily = new FontFamily("Segoe UI");
      doc.FontSize = 14;
      doc.PagePadding = new Thickness(16);

      var ctx = new ThemeContext(headingBrush, codeBrush, linkBrush, blockquoteBrush,
        codeBlockBg, inlineCodeBg, tableBorderBrush, tableHeaderBg, tableHeaderFg,
        tableEvenRowBg, tableOddRowBg, tableCellFg);

      foreach (var block in doc.Blocks)
        ApplyThemeToBlock(block, ctx);
    }

    private static void ApplyThemeToBlock(Block block, ThemeContext ctx)
    {
      if (block is Paragraph p)
      {
        // Headings: MdXaml sets larger FontSize on heading paragraphs.
        // Compress the size range toward the 14px baseline so H1 is less
        // extreme while the H1 > H2 > H3 hierarchy is still visible.
        if (p.FontSize > 15)
        {
          p.Foreground = ctx.HeadingBrush;
          p.FontWeight = FontWeights.Bold;
          p.FontSize = Math.Round(14 + (p.FontSize - 14) * 0.70);
        }

        // Code blocks: paragraphs with monospace font
        if (IsMonospaceFont(p.FontFamily))
        {
          p.Foreground = ctx.CodeBrush;
          p.Background = ctx.CodeBlockBg;
          p.Padding = new Thickness(10, 8, 10, 8);
          p.Margin = new Thickness(0, 6, 0, 6);
        }

        ApplyThemeToInlines(p.Inlines, ctx);
      }
      else if (block is Section section)
      {
        // Blockquotes: MdXaml wraps them in a Section, often with left border/margin
        if (section.BorderThickness.Left > 0 || section.Padding.Left > 5)
        {
          section.Foreground = ctx.BlockquoteBrush;
          section.FontStyle = FontStyles.Italic;
          section.BorderBrush = ctx.BlockquoteBrush;
          section.BorderThickness = new Thickness(3, 0, 0, 0);
          section.Padding = new Thickness(12, 4, 0, 4);
        }

        foreach (var innerBlock in section.Blocks)
          ApplyThemeToBlock(innerBlock, ctx);
      }
      else if (block is Table table)
      {
        ApplyThemeToTable(table, ctx);
      }
      else if (block is List list)
      {
        foreach (var item in list.ListItems)
          foreach (var itemBlock in item.Blocks)
            ApplyThemeToBlock(itemBlock, ctx);
      }
    }

    private static void ApplyThemeToTable(Table table, ThemeContext ctx)
    {
      table.BorderBrush = ctx.TableBorderBrush;
      table.BorderThickness = new Thickness(1);
      table.CellSpacing = 0;

      for (int g = 0; g < table.RowGroups.Count; g++)
      {
        var rowGroup = table.RowGroups[g];
        bool isHeaderGroup = (g == 0 && table.RowGroups.Count > 1);

        // Clear any MdXaml-set background on the group itself
        rowGroup.Background = null;
        rowGroup.Foreground = ctx.TableCellFg;

        for (int r = 0; r < rowGroup.Rows.Count; r++)
        {
          var row = rowGroup.Rows[r];

          if (isHeaderGroup)
          {
            row.Background = ctx.TableHeaderBg;
            row.Foreground = ctx.TableHeaderFg;
            row.FontWeight = FontWeights.SemiBold;
          }
          else
          {
            // Alternating body rows — explicit backgrounds to override MdXaml defaults
            row.Background = (r % 2 == 0) ? ctx.TableEvenRowBg : ctx.TableOddRowBg;
            row.Foreground = ctx.TableCellFg;
            row.FontWeight = FontWeights.Normal;
          }

          foreach (var cell in row.Cells)
          {
            // Override any cell-level background MdXaml may have set
            cell.Background = null;
            cell.BorderBrush = ctx.TableBorderBrush;
            cell.BorderThickness = new Thickness(0.5);
            cell.Padding = new Thickness(8, 4, 8, 4);

            foreach (var cellBlock in cell.Blocks)
            {
              if (cellBlock is Paragraph cellPara)
              {
                cellPara.Background = null;
                ApplyThemeToInlines(cellPara.Inlines, ctx);
              }
            }
          }
        }
      }
    }

    private static void ApplyThemeToInlines(InlineCollection inlines, ThemeContext ctx)
    {
      foreach (var inline in inlines)
      {
        if (inline is Hyperlink link)
        {
          link.Foreground = ctx.LinkBrush;
        }
        else if (inline is Run run && IsMonospaceFont(run.FontFamily))
        {
          run.Foreground = ctx.CodeBrush;
          run.Background = ctx.InlineCodeBg;
        }
        else if (inline is Span span)
        {
          if (span is Hyperlink hl)
            hl.Foreground = ctx.LinkBrush;
          else
            ApplyThemeToInlines(span.Inlines, ctx);
        }
      }
    }

    private static bool IsMonospaceFont(FontFamily font)
    {
      if (font == null) return false;
      var name = font.Source?.ToLowerInvariant() ?? "";
      return name.Contains("courier") || name.Contains("consolas") || name.Contains("mono");
    }

    private bool IsLightTheme()
    {
      if (Settings == null) return false;
      return Settings.ApplicationTheme == ApplicationTheme.Light
             || (Settings.ApplicationTheme.SupportsTint()
                 && ApplicationThemeExtensions.IsLightColor(Settings.ChromeTintColor));
    }

    private void MarkdownViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      // FlowDocumentScrollViewer scrolls too fast (by paragraph, not pixel line).
      // Override to scroll ~3 lines per notch, matching AvalonEdit.
      var sv = FindChildScrollViewer(this.MarkdownViewer);
      if (sv != null)
      {
        sv.ScrollToVerticalOffset(sv.VerticalOffset - (e.Delta * 0.6));
        e.Handled = true;
      }
    }

    private static ScrollViewer FindChildScrollViewer(DependencyObject parent)
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is ScrollViewer sv) return sv;
        var result = FindChildScrollViewer(child);
        if (result != null) return result;
      }
      return null;
    }

    private static SolidColorBrush FrozenBrush(string hex)
    {
      var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
      brush.Freeze();
      return brush;
    }

    /// <summary>Carries all theme colors through the recursive tree walk.</summary>
    private record ThemeContext(
      Brush HeadingBrush, Brush CodeBrush, Brush LinkBrush, Brush BlockquoteBrush,
      Brush CodeBlockBg, Brush InlineCodeBg,
      Brush TableBorderBrush, Brush TableHeaderBg, Brush TableHeaderFg,
      Brush TableEvenRowBg, Brush TableOddRowBg, Brush TableCellFg);
  }
}
