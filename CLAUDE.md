# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

eXeMeL is a WPF desktop XML editor for viewing, formatting, cleaning, and analyzing XML content. Originally authored at github.com/Grandpappy/eXeMeL. Built with .NET 9 (migrated from .NET Framework 4.5), targeting Windows as a single self-contained executable.

## Build & Run

**Build (PowerShell — required for dotnet/MSBuild):**
```powershell
cd src\eXeMeL
dotnet build eXeMeL\eXeMeL.csproj
```

**Run:**
```powershell
dotnet run --project eXeMeL\eXeMeL.csproj
```

**Run with file argument:**
```powershell
dotnet run --project eXeMeL\eXeMeL.csproj -- "path\to\file.xml"
```

**Publish single-file exe:**
```powershell
dotnet publish eXeMeL\eXeMeL.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```
Output: `eXeMeL\bin\Release\net9.0-windows\win-x64\publish\eXeMeL.exe` (~74 MB self-contained)

**Run tests:**
```powershell
dotnet test eXeMeL.Tests\eXeMeL.Tests.csproj
```

## Architecture

### MVVM with CommunityToolkit.Mvvm

The app follows MVVM using **CommunityToolkit.Mvvm** (migrated from MvvmLight). Key conventions:

- **ViewModelLocator** (`ViewModel/ViewModelLocator.cs`) registers `MainViewModel` as a singleton via `Microsoft.Extensions.DependencyInjection`. Views bind via `{Binding Source={StaticResource Locator}, Path=Main}` in XAML.
- **Messaging:** Cross-component communication uses `CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default`. There are ~12 message types in the `Messages/` folder. When adding new cross-cutting behavior, prefer a new message type over direct ViewModel coupling.
- **Commands:** `RelayCommand` from `CommunityToolkit.Mvvm.Input` for all command bindings.
- **Base class:** All ViewModels extend `ObservableObject`. Use `SetProperty()` for property setters and `OnPropertyChanged()` for change notifications.

### Key Components

| Layer | Location | Purpose |
|-------|----------|---------|
| Entry point | `App.xaml.cs` | Command-line arg handling, global exception handler |
| Main shell | `MainWindow.xaml.cs` | Custom WindowChrome title bar, AvalonEdit setup, XML folding, keyboard shortcuts, drag-drop |
| Core ViewModel | `ViewModel/MainViewModel.cs` | Orchestrates settings, editor, themes, syntax highlighting |
| Editor logic | `ViewModel/EditorViewModel.cs` | Clipboard ops, XML cleaning pipeline, snapshots, file I/O |
| XPath utility | `ViewModel/XmlUtility/XmlUtilityViewModel.cs` | XPath evaluation and tree navigation |
| XML cleaners | `ViewModel/XmlCleaners/` | Chain of cleaners (URL encoding, trim, newlines, VS artifacts, formatting) |
| Settings | `Model/Settings.cs`, `Model/SettingsIO.cs` | User preferences persisted as JSON to `%LOCALAPPDATA%\eXeMeL\settings.json` |
| Settings migration | `Model/SettingsMigrator.cs` | Auto-migrates old Registry settings to file on first run |
| Syntax themes | `Assets/SyntaxHighlightingSchemes/` | AvalonEdit `.xshd` files (VSBlue, Dark, Bright, Earthy, SolarizedDark) |
| Theme manager | `ViewModel/SyntaxHighlightManager.cs` | Swaps theme ResourceDictionaries using marker-key approach |

### XML Cleaning Pipeline

`EditorViewModel` runs XML text through a chain of cleaners in `ViewModel/XmlCleaners/`, each extending `XmlCleanerBase`. The pipeline handles URL decoding, whitespace normalization, Visual Studio paste artifacts, and formatting. Cleaners run asynchronously via `Task.Run`.

### UI Framework

- **MaterialDesignInXamlToolkit** — modern WPF control styling and theming
- **WindowChrome** — custom borderless title bar with min/max/close buttons
- **AvalonEdit 6.3** — text editor control with syntax highlighting, folding, and custom colorizers
- **Microsoft.Xaml.Behaviors.Wpf** — XAML interactivity/triggers

### Settings Storage

Settings persist as JSON to `%LOCALAPPDATA%\eXeMeL\settings.json` using `System.Text.Json`. On first run, `SettingsMigrator` checks for old Registry settings at `HKCU\Software\eXeMeL` and migrates them automatically.

### Theme System

Three application themes (Light, Dark, SolarizedDark) are defined as ResourceDictionaries in `Resources/`. Each contains a `IsEXeMeLTheme` marker key. `ApplicationThemeManager` swaps themes by finding and replacing the dictionary with this marker.

## Keyboard Shortcuts (defined in MainWindow.xaml)

F5 = Refresh/clean XML from clipboard, F2 = Toggle Editor/XPath mode, Ctrl+F = Find, Ctrl+S = Save, Ctrl+O = Open, Alt+0-9 = Fold levels, Alt+Shift+0-9 = Unfold levels

## NuGet Dependencies

- `CommunityToolkit.Mvvm` 8.4.0 — MVVM framework
- `Microsoft.Extensions.DependencyInjection` 9.0.0 — DI container
- `AvalonEdit` 6.3.0.90 — text editor control
- `MaterialDesignThemes` 5.1.0 — WPF Material Design styling
- `MaterialDesignColors` 3.1.0 — color palette
- `Microsoft.Xaml.Behaviors.Wpf` 1.1.77 — XAML behaviors

## Conventions

- All async work dispatches back to UI via `Utilities/UIThread.cs`
- `Utilities/PropertyObserver.cs` provides strongly-typed INotifyPropertyChanged observation (replaces MvvmFoundation.Wpf)
- The `StartupOptions.InitialFilePath` static property carries the file path from command-line args into the main window
- AvalonEdit 6.x enforces `TextDocument` thread affinity — documents must be created and accessed on the same thread
