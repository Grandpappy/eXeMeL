using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace eXeMeL.Utilities
{
  /// <summary>
  /// Factory for styled in-app dialogs with accent stripe and icon.
  /// </summary>
  public static class AppDialog
  {
    /// <summary>
    /// Shows a modal ContentDialog styled with an accent stripe at the top and an icon
    /// beside the message. Returns the button the user pressed.
    /// </summary>
    public static async Task<ContentDialogResult> ShowAsync(
      ContentDialogHost host,
      string title,
      string message,
      string primaryText,
      string secondaryText = null,
      string closeText = "Cancel",
      SymbolRegular icon = SymbolRegular.QuestionCircle48,
      CancellationToken cancellationToken = default)
    {
      var accentBrush = Application.Current.TryFindResource("AppAccentBrush") as Brush
                        ?? new SolidColorBrush(Color.FromRgb(0xD4, 0xAA, 0x00));
      var textBrush = Application.Current.TryFindResource("AppTextBrush") as Brush;

      // ── Title area: accent stripe + title text ──────────────────────────
      var titleRoot = new Grid();
      titleRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
      titleRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
      titleRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

      var stripe = new Border
      {
        Background = accentBrush,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        CornerRadius = new CornerRadius(2)
      };
      Grid.SetRow(stripe, 0);
      titleRoot.Children.Add(stripe);

      var titleText = new WpfTextBlock
      {
        Text = title,
        FontSize = 20,
        FontWeight = FontWeights.SemiBold,
      };
      if (textBrush != null) titleText.Foreground = textBrush;
      Grid.SetRow(titleText, 2);
      titleRoot.Children.Add(titleText);

      // ── Content area: icon + message ────────────────────────────────────
      var contentGrid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
      contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
      contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

      var symbolIcon = new SymbolIcon
      {
        Symbol = icon,
        FontSize = 40,
        Foreground = accentBrush,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 16, 0)
      };
      Grid.SetColumn(symbolIcon, 0);
      contentGrid.Children.Add(symbolIcon);

      var messageText = new WpfTextBlock
      {
        Text = message,
        TextWrapping = TextWrapping.Wrap,
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 14,
        MaxWidth = 360
      };
      if (textBrush != null) messageText.Foreground = textBrush;
      Grid.SetColumn(messageText, 1);
      contentGrid.Children.Add(messageText);

      // ── Build and show ───────────────────────────────────────────────────
      var dialog = new ContentDialog(host)
      {
        Title = titleRoot,
        Content = contentGrid,
        PrimaryButtonText = primaryText,
        CloseButtonText = closeText,
        PrimaryButtonAppearance = ControlAppearance.Primary
      };

      if (secondaryText != null)
        dialog.SecondaryButtonText = secondaryText;

      return await dialog.ShowAsync(cancellationToken);
    }
  }
}
