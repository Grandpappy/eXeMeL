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
      // ContentDialogTopOverlay has Padding="24,10" and the title ContentPresenter has
      // Margin="0,12,0,0", so the title element's top is 22px below the dialog's top edge.
      // Negative margins on titleRoot bleed the stripe through those 22px and to the side
      // edges. The overlay's own CornerRadius="8,8,0,0" clips the stripe's top corners
      // to the dialog's rounded shape automatically.
      var titleRoot = new Grid();
      titleRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });   // stripe
      titleRoot.RowDefinitions.Add(new RowDefinition { Height = new GridLength(18) });  // gap to title text
      titleRoot.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // title text
      titleRoot.Margin = new Thickness(-24, -22, -24, 0);

      var stripe = new Border { Background = accentBrush };
      Grid.SetRow(stripe, 0);
      titleRoot.Children.Add(stripe);

      var titleText = new WpfTextBlock
      {
        Text = title,
        FontSize = 20,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(24, 0, 24, 0)  // restore horizontal indent for text only
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
        MaxWidth = 480
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
