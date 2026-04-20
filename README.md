# eXeMeL

A desktop XML editor for Windows, built for developers who work with XML daily. eXeMeL reads XML from the clipboard on launch, cleans and formats it automatically, and provides syntax-highlighted editing with an integrated XPath authoring tool.

## Features

- **Clipboard-first workflow** — paste XML (or press F5) and it's instantly cleaned, decoded, and formatted
- **XML cleaning pipeline** — automatically handles URL-encoded XML, Visual Studio debug output, escaped quotes, whitespace, and XML fragments
- **Syntax highlighting** — five built-in color schemes (Bright, Earthy, Ethereal, Blue, Solarized Dark) with light and dark themes
- **XPath utility** — interactive XML tree view with XPath expression evaluation, element highlighting, and XPath generation via right-click
- **Document snapshots** — save and navigate between editing checkpoints
- **Encoded XML extraction** — right-click to decode and "delve into" HTML-encoded XML inside attributes
- **Code folding** — collapse/expand XML elements by level (Alt+0-9)
- **File support** — open, save, and drag-drop XML files
- **Single executable** — no installer required, just download and run

## Getting Started

### Download

Download either the intallable or stand-alone version of eXeMeL from the latest release.

### Build from source

Requires [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

```powershell
cd src\eXeMeL
dotnet build eXeMeL\eXeMeL.csproj
dotnet run --project eXeMeL\eXeMeL.csproj
```

### Publish single-file executable

```powershell
cd src\eXeMeL
dotnet publish eXeMeL\eXeMeL.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

Output: `eXeMeL\bin\Release\net9.0-windows\win-x64\publish\eXeMeL.exe`

### Run tests

```powershell
cd src\eXeMeL
dotnet test eXeMeL.Tests\eXeMeL.Tests.csproj
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| F5 | Refresh — read clipboard, clean, and display XML |
| F2 | Toggle between Editor and XPath Utility views |
| Ctrl+F | Find text |
| Ctrl+S | Save file |
| Ctrl+O | Open file |
| Alt+0-9 | Fold XML elements at level |
| Alt+Shift+0-9 | Unfold XML elements at level |
| Alt+- | Fold all |
| Alt+Shift+- | Unfold all |
| Escape | Return focus to editor |

## Settings

User settings are stored at `%LOCALAPPDATA%\eXeMeL2\settings.json`. If upgrading from an older version that stored settings in the Windows Registry (`HKCU\Software\eXeMeL`), they are migrated automatically on first launch.

## Technology Stack

- **Runtime:** .NET 9 (Windows)
- **UI Framework:** WPF with [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- **Text Editor:** [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) 6.3
- **MVVM:** [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) 8.4
- **Behaviors:** [Microsoft.Xaml.Behaviors.Wpf](https://github.com/microsoft/XamlBehaviorsWpf)

## Migration History

This application was originally built circa 2013 on .NET Framework 4.5. In April 2026 it was migrated to .NET 9 with the following changes:

### Framework and project system
- .NET Framework 4.5 to .NET 9 (`net9.0-windows`)
- Old-style `.csproj` to SDK-style
- `packages.config` NuGet to `PackageReference`

### Library replacements
| Before | After | Reason |
|--------|-------|--------|
| MvvmLight 4.1 | CommunityToolkit.Mvvm 8.4 | MvvmLight unmaintained since 2018 |
| MvvmFoundation.Wpf 1.0 | In-tree PropertyObserver | Unmaintained since 2012 |
| CommonServiceLocator 1.0 | Microsoft.Extensions.DependencyInjection 9.0 | Standard .NET DI |
| MahApps.Metro 0.12.1 | MaterialDesignInXamlToolkit 5.1 + WindowChrome | No .NET 9 support for MahApps 0.x |
| AvalonEdit 4.4 | AvalonEdit 6.3 | Modern .NET support (zero breaking changes) |
| Microsoft.Xaml.Behaviors.Wpf 1.1.19 | 1.1.77 | Version bump |

### Deployment
- ClickOnce installer removed (signing keys, manifests, bootstrapper config)
- Single self-contained executable via `dotnet publish` (~74 MB)
- Command-line file argument replaces ClickOnce activation

### Settings storage
- Windows Registry (`HKCU\Software\eXeMeL`) replaced with local JSON file (`%LOCALAPPDATA%\eXeMeL2\settings.json`)
- Automatic one-time migration from Registry on first run
- `System.Text.Json` replaces `DataContractJsonSerializer`

### Test coverage
- 116 xUnit tests added (103 passing, 13 skipped pending UI stabilization)
- XML cleaning pipeline, settings serialization, XPath utility, and behavioral workflow tests

## License

Originally published at [github.com/Grandpappy/eXeMeL](https://github.com/Grandpappy/eXeMeL).
