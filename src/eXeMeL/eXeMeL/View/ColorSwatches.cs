namespace eXeMeL.View
{
  public static class ColorSwatches
  {
    // Colors for window tinting (rich deep colors)
    public static string[] TintColors { get; } = new[]
    {
      "#1A1A2E", "#16213E", "#0F3460", "#1B1464",
      "#2C003E", "#512B58", "#4A0E4E", "#0D1B2A",
      "#003049", "#023E8A", "#0077B6", "#264653",
      "#2D6A4F", "#1B4332", "#3C1642", "#0B3D91",
      "#7B2D8E", "#541388", "#4C0070", "#1C0C27"
    };

    // Neutral text colors (whites, grays, off-whites)
    public static string[] TextColors { get; } = new[]
    {
      "#FFFFFF", "#F0F0F0", "#E0E0E0", "#CCCCCC",
      "#B0B0B0", "#A0A0A0", "#D4D4D4", "#C8C8C8",
      "#F5F5DC", "#FAFAFA", "#E8E8E8", "#D0D0D0"
    };

    // Editor background colors (dark and light options)
    public static string[] EditorBackgroundColors { get; } = new[]
    {
      "#1E1E1E", "#252525", "#2D2D2D", "#1A1A2E",
      "#0D1117", "#002B36", "#1B1B1B", "#171717",
      "#0A0A0A", "#2B2B2B", "#1C1C1C", "#242424",
      "#EDEDED", "#F5F5F5", "#FAFAFA", "#FFFFFF",
      "#FDF6E3", "#EEE8D5", "#E8E8E8", "#D0D0D0"
    };

    // Accent colors (vibrant, distinctive)
    public static string[] AccentColors { get; } = new[]
    {
      "#D4AA00", "#FFB900", "#FF8C00", "#F7630C",
      "#EA005E", "#E3008C", "#C239B3", "#881798",
      "#0078D4", "#0099BC", "#00B294", "#107C10",
      "#498205", "#DA3B01", "#EF6950", "#00CC6A",
      "#9A0089", "#4C4A48", "#FF4343", "#00B7C3"
    };
  }
}
