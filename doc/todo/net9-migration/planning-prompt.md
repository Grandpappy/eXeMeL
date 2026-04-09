# Expert Planning Prompt: net9-migration

## Role Definition

You are a **.NET platform and desktop application engineer** with deep expertise in SDK-style projects, WPF on .NET 9, NuGet modernization, legacy-to-modern .NET migration, and modern Windows desktop deployment technologies. Your mission is to produce a comprehensive, phased migration plan to move eXeMeL — a WPF XML editor currently on .NET Framework 4.5 — to .NET 9 (or .NET 8 if dependency support forces it), replace ClickOnce with a single self-contained `.exe`, modernize the UI, and ensure all existing functionality is preserved for daily users.

## Reading Order

Before producing the plan, read these materials in order. Understand each before proceeding to the next:

1. `src/eXeMeL/eXeMeL/eXeMeL.csproj` — Old-style csproj with .NET Framework 4.5 target, ClickOnce configuration, signing manifests, file associations, and all project references. This is the primary artifact that must be converted to SDK-style.
2. `src/eXeMeL/eXeMeL/packages.config` — All 7 NuGet dependencies. Critical for understanding which libraries need replacement vs upgrade.
3. `src/eXeMeL/eXeMeL/MainWindow.xaml` — Primary UI using MahApps.Metro MetroWindow, AvalonEdit TextEditor, Flyouts, WindowCommands, keyboard bindings, and VisualStateManager for Editor/XmlUtility mode switching.
4. `src/eXeMeL/eXeMeL/View/XmlUtilityView.xaml` — XPath utility UI. This is a critical feature for users — complex tree rendering with DataTemplates for ElementViewModel/AttributeViewModel, XPath highlighting, expand/collapse, context menus for XPath operations.
5. `src/eXeMeL/eXeMeL/ViewModel/XmlUtility/XmlUtilityViewModel.cs` — XPath utility logic with async tree parsing, cancellation-based XPath evaluation queuing, and Messenger integration. Most complex ViewModel in the project.
6. `src/eXeMeL/eXeMeL/ViewModel/EditorViewModel.cs` — Core editor logic: clipboard operations (Refresh = paste from clipboard and clean XML), XML cleaning pipeline, document snapshots, file I/O, encoded XML extraction. The clipboard-on-startup behavior (RefreshCommand) is a must-have feature.
7. `src/eXeMeL/eXeMeL/ViewModel/MainViewModel.cs` — Root ViewModel orchestrating Settings, EditorViewModel, XmlUtilityViewModel, SyntaxHighlightingManager, ApplicationThemeManager, and Messenger registrations.
8. `src/eXeMeL/eXeMeL/Model/Settings.cs` — User settings with DataContract serialization, INotifyPropertyChanged, theme-aware brush resolution via custom attributes on enum values.
9. `src/eXeMeL/eXeMeL/Model/SettingsIO.cs` — Settings persistence using DataContractJsonSerializer to Windows Registry at `HKCU\Software\eXeMeL`.
10. `src/eXeMeL/eXeMeL/Model/RegistryAccess.cs` — Registry key management for settings storage.
11. `src/eXeMeL/eXeMeL/Model/SyntaxHighlightingStyleEnum.cs` — Enum with 5 syntax highlighting schemes (Bright, Earthy, Ethereal/Dark, Blue/VSBlue, Solarized), each decorated with custom attributes mapping to embedded `.xshd` resources and theme-specific brush colors.
12. `src/eXeMeL/eXeMeL/Model/ApplicationThemeEnum.cs` — 3 application themes (Light, Dark, SolarizedDark) mapped to ResourceDictionary paths via custom attributes.
13. `src/eXeMeL/eXeMeL/ViewModel/SyntaxHighlightManager.cs` — Loads `.xshd` syntax highlighting from embedded resources, watches settings changes via MvvmFoundation.Wpf PropertyObserver. Also contains ApplicationThemeManager which swaps ResourceDictionaries at runtime.
14. `src/eXeMeL/eXeMeL/App.xaml.cs` — Application startup with ClickOnce ActivationArguments handling and global exception handler.
15. `src/eXeMeL/eXeMeL/ViewModel/XmlCleaners/` — Read all 8 cleaner files. Chain-of-responsibility XML cleaning pipeline (URL encoding, trim, newlines, surrounding garbage, Visual Studio artifacts, VBScript artifacts, added root, formatting).
16. `CLAUDE.md` — Architecture notes for the repository.

## Architecture Constraints

The plan MUST respect these constraints:

- **Behavioral parity** — All existing features must work identically after migration. Consult the user before cutting any feature.
- **Clipboard-on-startup is sacred** — The behavior where opening the application reads clipboard content, cleans it as XML, and displays it in the editor is a critical daily-use feature that MUST be preserved exactly as it works today.
- **XPath utility preservation** — The XPath authoring and tree navigation tool is a particularly valued feature. It must be fully functional after migration.
- **Settings migration** — User settings currently stored in Windows Registry (`HKCU\Software\eXeMeL`) must be automatically migrated to the new storage location on first run. No user data loss.
- **Syntax highlighting continuity** — All 5 existing `.xshd` schemes (Bright, Earthy, Dark/Ethereal, VSBlue, SolarizedDark) must be preserved. New schemes may be added.
- **Theme continuity** — All 3 application themes (Light, Dark, SolarizedDark) must continue to work.
- **Target framework flexibility** — Target .NET 9. If any critical dependency only supports up to .NET 8, fall back to .NET 8. Document which dependency forced the decision.
- **Library compatibility strategy** — For each dependency: (1) prefer direct upgrade if .NET 9/8 version exists, (2) if library is dead, evaluate replacement with a feature-equivalent modern library, (3) if no replacement exists, evaluate forking the open-source code and porting it in-tree (license permitting).
- **Test-first** — Create tests that validate existing behavior BEFORE making migration changes, so regressions can be caught.
- **Single self-contained executable** — The final output must be a single `.exe` that runs without installation on any Windows PC. No installer, no ClickOnce, no runtime prerequisites for the end user.
- **Windows-only** — Cross-platform support is not required. Targeting Windows is acceptable.

## Scope

### In Scope
- .NET Framework 4.5 → .NET 9 (or 8) framework migration
- Old-style csproj → SDK-style csproj conversion
- packages.config → PackageReference NuGet migration
- MvvmLight replacement (dead library — CommunityToolkit.Mvvm is the spiritual successor)
- MvvmFoundation.Wpf replacement (dead library — only used for PropertyObserver)
- MahApps.Metro upgrade or replacement with a modern clean WPF design
- AvalonEdit upgrade (or replacement if not ported)
- Microsoft.Xaml.Behaviors.Wpf upgrade
- UI modernization — the UI does not need to match the current design, but must be clean and modern. Research current WPF design patterns.
- ClickOnce removal — remove all signing, manifests, bootstrapper config from csproj
- App startup rework — replace ClickOnce ActivationArguments with standard command-line args
- Settings storage modernization — move from Windows Registry to modern local file storage (research current best practices for where desktop apps should store user settings)
- Settings migration — on first run, detect and migrate existing Registry settings to the new location
- Single-file self-contained publish configuration
- Pre-migration test creation for all existing functionality
- Syntax highlighting scheme preservation (all 5 existing .xshd files)
- Application theme preservation (all 3 themes)
- File drag-drop support preservation
- File open/save dialog preservation
- Keyboard shortcut preservation

### Out of Scope — Do NOT Plan For
- New features beyond what exists today (except new syntax highlighting schemes, which are welcome)
- Cross-platform support
- Cloud-based settings storage
- CI/CD pipeline setup
- Code signing (was only needed for ClickOnce)

## Do NOT

These approaches, patterns, and shortcuts are explicitly prohibited:

- **No web-hosted technologies** — The app must run entirely on the client machine. No web server, no browser requirement.
- **No browser-in-browser** — Electron-style frameworks are acceptable only if they don't require the user to open a browser. But pure WPF is preferred.
- **No cross-platform targeting** — Do not use MAUI, Avalonia, or Uno Platform. WPF on Windows is the target.
- **No multiple executables** — The output must be a single `.exe`. No separate installer, no companion processes.
- **No cloud settings** — Settings must be stored locally on the user's machine.
- **No splitting the project** — Keep this as a single-project solution unless there is a compelling architectural reason to split (e.g., a test project is fine).
- **No "follow best practices" hand-waving** — Every recommendation must be specific and actionable.

## Output Format

Produce a master plan with numbered sections. Each section MUST contain ALL of the following:

1. **Title** — `## {N}. {Section Title}` (number sequentially starting at 1)
2. **Summary** — One paragraph describing the section's purpose and approach
3. **Deliverables** — Bulleted list of concrete, verifiable deliverables and tasks
4. **Dependencies** — Which other sections (by number) must complete before this one can start. If none, state "None"
5. **Complexity** — One of: S (< 1 day), M (1-3 days), L (3-5 days), XL (5+ days). Calibrate to THIS project, not generic estimates
6. **Key Risks** — Risks and decision points specific to this section. If the section involves a choice between approaches, state the options and your recommendation

## Section Numbering Convention

Number sections sequentially starting at 1. These numbers will be used as prefixes for todo files:
- `{section#}__{name}.md` (double underscore) = primary tracker for the section
- `{section#}_{step#}_{name}.md` (single underscores) = ordered subtask within the section
- Subtasks sharing the same step number can run in parallel

## Structural Requirements

The plan must be organized into three major phases:

### Phase A: Test Foundation
Build a test harness and tests that validate existing behavior BEFORE any migration work begins. This is the safety net.

### Phase B: Technology Landscape Evaluation
For each dependency that needs replacement or upgrade, provide:
- Current library name and version
- Migration status (.NET 9 support? .NET 8? Dead?)
- Recommended replacement with specific package name and version
- Feature gap analysis (what works differently, what's missing)
- Links/references where the user can read more about each option
- If multiple options exist, present them with pros/cons and a recommendation

This phase should be interactive — present findings and get user buy-in before proceeding to migration.

### Phase C: Phased Migration
Break migration into sequential phases where each phase produces a compilable, runnable application. Early phases should be designed for hands-on user validation. Later phases can be more agent-automatable.

Each migration phase must end with: "At this point, the application compiles and runs with [these features working] and [these features temporarily broken/changed]."
