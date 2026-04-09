# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

eXeMeL is a WPF desktop XML editor for viewing, formatting, cleaning, and analyzing XML content. Originally authored at github.com/Grandpappy/eXeMeL. Built with .NET Framework 4.5 (migration to 4.8 in progress on `move-to-net48` branch).

## Build & Run

**Build (PowerShell — required for MSBuild):**
```powershell
cd src\eXeMeL
msbuild eXeMeL.sln /p:Configuration=Release
```

**Run:** Open `src/eXeMeL/eXeMeL.sln` in Visual Studio and F5, or execute `bin/Release/eXeMeL.exe` after building.

**NuGet restore:** Packages are in `packages.config` format (not PackageReference). Restore with `nuget restore src\eXeMeL\eXeMeL.sln` if needed.

**No test projects exist** in the solution currently.

## Architecture

### MVVM with MvvmLight

The app follows strict MVVM using **MvvmLight** (GalaSoft). Key conventions:

- **ViewModelLocator** (`ViewModel/ViewModelLocator.cs`) registers `MainViewModel` as a singleton via `SimpleIoc`. Views bind via `{Binding Source={StaticResource Locator}, Path=Main}` in XAML.
- **Messaging:** Cross-component communication uses `GalaSoft.MvvmLight.Messaging.Messenger`. There are ~12 message types in the `Messages/` folder that coordinate events like document refresh, status updates, and clipboard operations. When adding new cross-cutting behavior, prefer a new message type over direct ViewModel coupling.
- **Commands:** `RelayCommand` from MvvmLight for all command bindings.

### Key Components

| Layer | Location | Purpose |
|-------|----------|---------|
| Entry point | `App.xaml.cs` | ClickOnce activation args, global exception handler |
| Main shell | `MainWindow.xaml.cs` | AvalonEdit setup, XML folding, keyboard shortcuts, drag-drop |
| Core ViewModel | `ViewModel/MainViewModel.cs` | Orchestrates settings, editor, themes, syntax highlighting |
| Editor logic | `ViewModel/EditorViewModel.cs` | Clipboard ops, XML cleaning pipeline, snapshots, file I/O |
| XPath utility | `ViewModel/XmlUtilityViewModel.cs` | XPath evaluation and tree navigation |
| XML cleaners | `ViewModel/XmlCleaners/` | Chain of cleaners (URL encoding, trim, newlines, VS artifacts, formatting) |
| Settings | `Model/Settings.cs`, `Model/SettingsIO.cs` | User preferences persisted as JSON to the Windows Registry |
| Syntax themes | `Assets/SyntaxHighlightingSchemes/` | AvalonEdit `.xshd` files (VSBlue, Dark, Bright, SolarizedDark) |

### XML Cleaning Pipeline

`EditorViewModel` runs XML text through a chain of cleaners in `ViewModel/XmlCleaners/`, each extending `XmlCleanerBase`. The pipeline handles URL decoding, whitespace normalization, Visual Studio paste artifacts, and formatting. Cleaners run asynchronously via `Task.Run`.

### UI Framework Dependencies

- **AvalonEdit** — text editor control with syntax highlighting and folding
- **MahApps.Metro** — modern WPF chrome and theming
- **Microsoft.Xaml.Behaviors.Wpf** — XAML interactivity/triggers

### Deployment

ClickOnce deployment is configured in the csproj (publish URL, update settings, signing manifest). The `Properties/app.manifest` controls trust and permissions.

## Keyboard Shortcuts (defined in MainWindow.xaml.cs)

F5 = Refresh/clean XML, F2 = Toggle edit mode, Ctrl+F = Find, Ctrl+S = Save snapshot

## Conventions

- All async work dispatches back to UI via `Utilities/UIThread.cs`
- Settings persist to the Windows Registry (not file-based config)
- The `StartupOptions.InitialFilePath` static property carries the file path from ClickOnce activation into the main window
