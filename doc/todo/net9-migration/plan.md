# eXeMeL Migration Plan: .NET Framework 4.5 to .NET 9

---

## Phase A: Test Foundation

---

## 1. Create Test Project and XML Cleaning Pipeline Tests

**Summary**

Establish a new xUnit test project targeting .NET Framework 4.5 (matching the current application target) that validates the XML cleaning pipeline's behavior. The cleaning pipeline is the most testable and most critical logic in the application — it is the foundation of the clipboard-on-startup feature. Each of the 8 cleaners (`UrlEncodingCleaner`, `TrimCleaner`, `NewLineCleaner`, `SurroundingGarbageCleaner`, `VisualStudioCleaner`, `VisualStudioVBScriptCleaner`, `AddedRootCleaner`, `FormatCleaner`) must have test coverage for their individual behavior, and the full pipeline as orchestrated by `EditorViewModel.CleanXmlIfPossibleAsync` must have integration tests that exercise real-world clipboard input scenarios.

**Deliverables**

- Create `src/eXeMeL/eXeMeL.Tests/eXeMeL.Tests.csproj` targeting .NET Framework 4.5, referencing xUnit 2.x (the last version supporting net45) and the main eXeMeL project
- Unit tests for `UrlEncodingCleaner`: verify URL-encoded XML (`%3C`, `%3E`, `%22`) is decoded correctly
- Unit tests for `TrimCleaner`: verify leading/trailing whitespace is removed
- Unit tests for `NewLineCleaner`: verify `Environment.NewLine` characters are stripped
- Unit tests for `SurroundingGarbageCleaner`: verify text before first `<` and after last `>` is stripped; verify no-op when no angle brackets present
- Unit tests for `VisualStudioCleaner`: verify escaped quotes (`\"`) are unescaped to `"`
- Unit tests for `VisualStudioVBScriptCleaner`: verify double-quotes (`""`) in VBScript-style XML are reduced to single quotes, but double-quotes inside valid attribute value contexts (e.g., `=""value""`) are preserved
- Unit tests for `AddedRootCleaner`: verify valid XML passes through; verify XML fragments get wrapped in `<AddedRoot>`; verify completely unparseable text sets `ErrorMessage`
- Unit tests for `FormatCleaner`: verify it is a no-op (current implementation is empty — document this)
- Integration tests for `XmlCleanerContext` pipeline: pass URL-encoded, whitespace-surrounded, Visual Studio pasted XML through the full chain and verify output
- Test the `XmlShouldBeCleaned` predicate logic in `EditorViewModel` (private method — test indirectly through `CleanXmlIfPossibleAsync`)
- Test `EncodedXmlExtractor.GetDecodedXmlAroundIndexAsync`: verify HTML-decoded text extraction from attribute values and text elements

**Dependencies**

- None

**Complexity**

- M (1-3 days)

**Key Risks**

- The `VisualStudioVBScriptCleaner` regex (`(=)?(\"\")(\\s|/?\\s?>)?`) has nuanced match group logic — tests must cover edge cases where all groups match (preserving `""`) vs. partial matches (collapsing to `"`). Manually verify the existing behavior before writing assertions.
- `EditorViewModel` constructor uses `IsInDesignMode` from MvvmLight, which may behave unexpectedly in test contexts. The cleaners themselves are independently testable via `XmlCleanerContext`, so prioritize testing cleaners directly.
- `FormatCleaner.CleanXml` is a no-op. This is intentional (placeholder). Document it in a test as `[Fact] FormatCleaner_DoesNotModifyInput`.

---

## 2. Create Settings Serialization and Registry I/O Tests

**Summary**

Create tests that validate the settings round-trip: serialize a `Settings` object to JSON via `DataContractJsonSerializer`, write it to a mock or real registry key, read it back, and verify all properties survive the journey. This is critical because the migration will change the storage mechanism, and we must be able to prove the data is preserved.

**Deliverables**

- Unit tests for `Settings` default constructor: verify defaults (`ShowEditorLineNumbers=true`, `WrapEditorText=true`, `EditorFontSize=16`, `SyntaxHighlightingStyle=Light_Earthy`, `ApplicationTheme=Light`, `FontFamily="Consolas"`)
- Round-trip serialization test: create a `Settings` with non-default values, serialize with `DataContractJsonSerializer`, deserialize, assert all `[DataMember]` properties match
- Test `Settings.GetBrushForCurrentTheme` (via public `UpdateBrushes` triggered by setting `SyntaxHighlightingStyle`): verify that each combination of `SyntaxHighlightingStyle` + `ApplicationTheme` resolves to a non-null `Brush` and not the fallback red brush
- Snapshot the JSON format produced by current `DataContractJsonSerializer` for a known `Settings` instance — save this as a test fixture file. This will be used during migration to verify the new settings reader can still parse the old format.
- Document the exact Windows Registry path: `HKCU\Software\eXeMeL`, value name: `Settings`, value type: `REG_SZ` containing JSON

**Dependencies**

- None (can run in parallel with Section 1)

**Complexity**

- S (< 1 day)

**Key Risks**

- The `AssociatedThemeBrushAttribute` constructor calls `BrushConverter().ConvertFromString()`, which requires a WPF context. Tests that exercise `Settings.SyntaxHighlightingStyle` setter (which calls `UpdateBrushes`) will need to run in an STA thread. Use `[STAFact]` from `Xunit.StaFact` NuGet package, or isolate brush resolution tests.
- `DataContractJsonSerializer` produces a specific JSON shape (e.g., enum values as integers, not strings). Capture this shape exactly in fixture files.

---

## 3. Create XPath Utility and Tree Navigation Tests

**Summary**

Create tests for the XPath utility subsystem: `ElementViewModel` tree construction, `XmlUtilityOperations` XPath generation (from root and from start element), and `XmlUtilityViewModel` XPath evaluation with result highlighting. The XPath utility is a particularly valued feature and must have regression coverage before migration.

**Deliverables**

- Unit tests for `ElementViewModel`: given an `XElement`, verify `ChildElements`, `Attributes`, `InnerText`, `HasInnerText`, `Name`, `IsExpanded` defaults
- Unit tests for `ElementViewModel.GetElementAndAllDescendents`: verify flat list contains all nested elements
- Unit tests for `ElementViewModel.CollapseAllChildElements` and `CollapseAllChildElementsExcept`: verify `IsExpanded` states
- Unit tests for `XmlUtilityOperations.HandleBuildXPathFromRootMessage`: given a known XML tree, verify generated XPath string for leaf elements
- Unit tests for `XmlUtilityOperations.HandleBuildXpathFromStartMessage`: given a start element partway down the tree, verify relative XPath with `../` prefixes
- Integration test for `XmlUtilityViewModel.ParseDocumentText`: pass valid XML, verify `IsXmlValid=true` and `Root` is populated; pass invalid XML, verify `IsXmlValid=false`
- Test `XmlUtilityViewModel.UpdateElementsInXPath`: set `XPath` property, verify `IsXPathTarget` is set on matching elements

**Dependencies**

- None (can run in parallel with Sections 1 and 2)

**Complexity**

- M (1-3 days)

**Key Risks**

- `XmlUtilityViewModel` uses `GalaSoft.MvvmLight.Messaging.Messenger.Default` for cross-component communication. Tests must either reset the messenger between runs or use isolated instances. MvvmLight's `Messenger.Default` is a singleton — call `Messenger.Reset()` in test setup/teardown.
- The `UpdateElementsInXPath` method uses a custom cancellation-based action queue (`AddNewElementUpdateAction` / `CompleteCurrentElementUpdateAction`). Tests must account for asynchronous execution — use `Task.Delay` or polling with timeout to wait for completion.
- `XmlUtilityOperations` registers message handlers on `Messenger.Default` in its constructor. Tests must trigger those messages or call internal methods directly (they are private — consider making them `internal` with `[InternalsVisibleTo]`).

---

## 4. Create Behavioral Smoke Tests for Core User Workflows

**Summary**

Create high-level integration tests that simulate the critical user workflows without requiring a live WPF window. These tests exercise the ViewModel layer end-to-end: startup clipboard read, file open, file save, snapshot creation/navigation, editor mode toggling, and encoded XML extraction. These serve as the primary regression safety net during migration.

**Deliverables**

- Test: clipboard-on-startup workflow — populate clipboard with raw XML, invoke `EditorViewModel.RefreshCommand`, verify `Document.Text` contains cleaned/formatted XML and `DocumentRefreshCompleted` message is sent
- Test: file open workflow — create temp XML file, invoke `EditorViewModel.OpenFileAsync`, verify `Document.Text` matches file contents, `IsContentFromFile=true`, `FilePath` is set
- Test: snapshot creation — invoke `CreateSnapshotCommand`, verify `Snapshots` collection grows, identifiers are "Original" / "Current"
- Test: snapshot navigation — create multiple snapshots, invoke `ChangeToSnapshotCommand`, verify `Document` switches to the correct snapshot's document
- Test: snapshot clearing after edit — after creating snapshots, call `ClearSnapshotsAfterDocument`, verify later snapshots are removed
- Test: editor mode toggle — verify `ToggleEditorModeCommand` flips between `EditorMode.Editor` and `EditorMode.XmlUtility`, and that `XmlUtility.DocumentText` is populated when switching to XmlUtility mode
- Test: delve into decoded XML — set document text with an attribute containing HTML-encoded XML, set `CaretPosition` inside the attribute, invoke `DelveIntoDecodedXmlFromCursorPositionCommand`, verify a new snapshot is created with decoded XML
- Add `[assembly: InternalsVisibleTo("eXeMeL.Tests")]` to `AssemblyInfo.cs` so tests can access `internal` types like the cleaners

**Dependencies**

- Section 1, Section 2, Section 3 (test infrastructure must exist)

**Complexity**

- M (1-3 days)

**Key Risks**

- `Clipboard.GetText()` and `Clipboard.SetText()` require STA thread and a running WPF message pump. In test environments, use `[STAFact]` and set clipboard text before invoking the refresh command. If clipboard access is flaky in CI, consider extracting clipboard access behind an interface for testability (this is a small refactor but worth doing before migration).
- `EditorViewModel.RefreshCommand_Execute` is `async void` — tests cannot directly await it. Use `EditorViewModel.RefreshComplete` event to detect completion, or refactor to expose an awaitable method.

---

## Phase B: Technology Landscape Evaluation

---

## 5. Dependency Audit and Replacement Research

**Summary**

Systematically evaluate each of the 7 NuGet dependencies plus the 2 framework-provided libraries for .NET 9 compatibility. For each, determine whether a direct upgrade, replacement, or in-tree fork is needed. Present findings with specific package names, version numbers, and feature gap analysis. This section is interactive — findings should be reviewed before proceeding.

**Deliverables**

- **Dependency Evaluation Matrix:**

| # | Current Package | Current Version | .NET 9 Status | Recommendation | Replacement Package | Notes |
|---|----------------|----------------|---------------|----------------|--------------------|----|
| 1 | `MvvmLightLibs` | 4.1.27.0 | Dead. No .NET Core/.NET 5+ support. Last release 2014. | Replace | `CommunityToolkit.Mvvm` 8.x | Spiritual successor by same community. Source-generated `[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`, `WeakReferenceMessenger`. |
| 2 | `MvvmLight` | 4.1.27.0 | Dead (same as above). | Replace (same as #1) | `CommunityToolkit.Mvvm` 8.x | `Messenger.Default` → `WeakReferenceMessenger.Default`. `RelayCommand` → `RelayCommand`. `ViewModelBase` → `ObservableObject`. `SimpleIoc` → `Microsoft.Extensions.DependencyInjection`. |
| 3 | `CommonServiceLocator` | 1.0 | Dead. Only used as MvvmLight dependency for `SimpleIoc`. | Remove | `Microsoft.Extensions.DependencyInjection` 9.x | Used only in `ViewModelLocator` via `ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default)`. Replace with direct DI container registration. |
| 4 | `MvvmFoundation.Wpf` | 1.0.0 | Dead. No .NET Core support. Net40 only. | Replace | `CommunityToolkit.Mvvm` (partial) + inline `PropertyObserver` | Only used for `PropertyObserver<T>` (in `SettingsWatcherBase` and `MainWindow.xaml.cs`). Port the ~100-line `PropertyObserver` class in-tree (MIT license), or replace with direct `INotifyPropertyChanged` subscriptions. |
| 5 | `AvalonEdit` | 4.4.0.9727 | Active. Current version: `6.3.0.90` on NuGet. Supports `net6.0-windows` and above. | Upgrade | `AvalonEdit` 6.3.x | API is largely backward-compatible. `.xshd` format is the same. |
| 6 | `MahApps.Metro` | 0.12.1.0 | Partially alive. Current 2.4.10 supports `net462`+. **No official .NET 6/7/8/9 support.** Version 3.0 in development but unreleased. | Replace | Custom WPF UI or `MaterialDesignInXamlToolkit` | See Key Risks below. |
| 7 | `Microsoft.Xaml.Behaviors.Wpf` | 1.1.19 | Active. Current version `1.1.77` supports `net6.0-windows`+. | Upgrade | `Microsoft.Xaml.Behaviors.Wpf` 1.1.x | Drop-in upgrade. Same namespace, same API. |

- **Feature Gap Analysis for MvvmLight → CommunityToolkit.Mvvm:**
  - `ViewModelBase` → `ObservableObject` (direct mapping)
  - `ViewModelBase.Set(() => Property, ref field, value)` → `SetProperty(ref field, value)` (nearly identical)
  - `ViewModelBase.RaisePropertyChanged(() => Property)` → `OnPropertyChanged(nameof(Property))`
  - `RelayCommand` / `RelayCommand<T>` → `RelayCommand` / `RelayCommand<T>` (same names, compatible API)
  - `Messenger.Default.Register<T>(recipient, action)` → `WeakReferenceMessenger.Default.Register<T>(recipient, handler)` (handler signature changes from `Action<T>` to `MessageHandler<object, T>`)
  - `Messenger.Default.Send<T>(message)` → `WeakReferenceMessenger.Default.Send(message)`
  - `SimpleIoc` → `Microsoft.Extensions.DependencyInjection.ServiceCollection` / `ServiceProvider`
  - `ViewModelBase.IsInDesignMode` → use `DesignerProperties.GetIsInDesignMode(new DependencyObject())`

- **Feature Gap Analysis for MahApps.Metro removal:**
  - `MetroWindow` → standard `Window` with custom chrome (`WindowChrome` for borderless title bar)
  - `Flyout` / `FlyoutsControl` → custom `Popup` or sliding panel UserControl
  - `WindowCommands` → custom toolbar in title bar area
  - `ToggleSwitch` → custom styled `ToggleButton` or third-party control
  - `SaveWindowPosition="True"` → implement manually (save/restore position in settings)
  - Dynamic resources (`AccentColor`, `AccentColorBrush`, `BlackBrush`, `WhiteBrush`, `Gray2Brush`, `Gray4Brush`, `LabelTextBrush`) → define equivalents in custom theme dictionaries

- **Research links for user review:**
  - CommunityToolkit.Mvvm migration guide: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
  - AvalonEdit on .NET Core: https://github.com/icsharpcode/AvalonEdit
  - MahApps.Metro .NET Core status: https://github.com/MahApps/MahApps.Metro/issues
  - WPF `WindowChrome` docs: https://learn.microsoft.com/en-us/dotnet/api/system.windows.shell.windowchrome
  - `Microsoft.Xaml.Behaviors.Wpf`: https://github.com/microsoft/XamlBehaviorsWpf
  - MaterialDesignInXamlToolkit: https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit

**Dependencies**

- None (research can happen in parallel with Phase A test writing)

**Complexity**

- M (1-3 days) — primarily research, not code

**Key Risks**

- **MahApps.Metro is the highest-risk dependency.** No official .NET 9 support. The recommendation is to **remove MahApps entirely** and replace with custom WPF styling. Reasons: (1) no official .NET 9 support timeline, (2) the app only uses a small subset (`MetroWindow`, `Flyout`, `ToggleSwitch`, `WindowCommands`), (3) modern WPF with `WindowChrome` can achieve the same borderless window look. The UI modernization goal aligns with this approach.
- **Alternative for MahApps:** If user prefers a third-party UI framework, evaluate `MaterialDesignInXamlToolkit` (active, supports .NET 8+, MIT license) or `HandyControl` (active, supports .NET 8+). These provide modern controls but require significant XAML rework regardless.
- **PropertyObserver replacement** is low risk. The `PropertyObserver<T>` pattern is used in exactly 3 places: `SyntaxHighlightingManager`, `ApplicationThemeManager`, and `MainWindow.xaml.cs`. A simple inline implementation or direct `PropertyChanged` subscription is sufficient.

---

## Phase C: Phased Migration

---

## 6. Convert to SDK-Style Project and Retarget to .NET 9

**Summary**

Convert the old-style `.csproj` to an SDK-style project file targeting `net9.0-windows`. This is the foundational migration step. Remove all ClickOnce configuration, signing manifests, bootstrapper packages, and file association declarations. Configure `UseWPF`, set up embedded resources, and establish the single-file publish profile. The project should compile (possibly with errors from missing packages) after this step.

**Deliverables**

- Replace `src/eXeMeL/eXeMeL/eXeMeL.csproj` with a new SDK-style project file containing:
  - `<TargetFramework>net9.0-windows</TargetFramework>`
  - `<OutputType>WinExe</OutputType>`
  - `<UseWPF>true</UseWPF>`
  - `<RootNamespace>eXeMeL</RootNamespace>`
  - `<AssemblyName>eXeMeL</AssemblyName>`
  - `<ApplicationIcon>Assets\eXeMeL Icon.ico</ApplicationIcon>`
  - Embedded resources for all 5 `.xshd` files and `ChangeLog.txt`
  - `<PublishSingleFile>true</PublishSingleFile>`, `<SelfContained>true</SelfContained>`, `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`, `<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>` as conditional publish properties
- Remove all of the following from the project:
  - ClickOnce settings (`PublishUrl`, `Install`, `InstallFrom`, `UpdateEnabled`, `UpdateMode`, etc.)
  - Manifest signing (`ManifestCertificateThumbprint`, `ManifestKeyFile`, `GenerateManifests`, `SignManifests`)
  - BootstrapperPackage items
  - FileAssociation items
  - `ApplicationManifest`, `TargetZone` properties
  - All `.pfx` key files (delete from repo)
  - `packages.config` (replaced by PackageReference)
  - `Properties/app.manifest` (preserve DPI settings if present, move to csproj)
  - `Properties/AssemblyInfo.cs` (SDK-style generates this; move custom attributes to csproj)
  - `Properties/Settings.settings` and `Properties/Settings.Designer.cs` (if unused)
  - `MVVMLight.Nuget.Readme.txt`
- Add PackageReference stubs (versions from Section 5):
  - `CommunityToolkit.Mvvm`
  - `AvalonEdit`
  - `Microsoft.Xaml.Behaviors.Wpf`
- Update solution file if needed
- Verify the project loads in Visual Studio (may not compile yet — expected)

**Dependencies**

- Section 5 (must know target packages before adding PackageReferences)

**Complexity**

- M (1-3 days)

**Key Risks**

- SDK-style projects auto-include all `.cs` files. After conversion, verify no unexpected files are included (e.g., `obj/` artifacts).
- Embedded resource names in SDK-style projects use a different naming convention. The `.xshd` files are referenced by full name (e.g., `eXeMeL.Assets.SyntaxHighlightingSchemes.Bright.xshd`). Verify SDK-style produces the same names, or update `SyntaxHighlightingManager.GetSyntaxHighlighting()`. Use `<LogicalName>` on `<EmbeddedResource>` items if needed.
- `Resources\DarkThemeColors.xaml` and `Resources\ThemeColors.xaml` are declared as `<Resource>` (not `<Page>`) in the old csproj. SDK-style auto-includes XAML as `<Page>`. Explicitly set these as `<Resource>` if `pack://` URI resolution depends on it.

---

## 7. Replace MvvmLight and MvvmFoundation with CommunityToolkit.Mvvm

**Summary**

This is the largest code-change section. Replace all MvvmLight and MvvmFoundation usage across every ViewModel and code-behind file with CommunityToolkit.Mvvm equivalents. This affects every ViewModel class, the `ViewModelLocator`, and message handler registrations throughout the app.

**Deliverables**

- Add `<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />` (or latest 8.x)
- Add `<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />`
- Remove all MvvmLight, MvvmFoundation, and CommonServiceLocator package references
- **ViewModelLocator rewrite:**
  - Replace `SimpleIoc.Default` / `ServiceLocator` with `ServiceCollection` and `ServiceProvider`
  - Register `MainViewModel` as singleton
  - Expose `Main` property via `IServiceProvider.GetRequiredService<MainViewModel>()`
- **Base class migration** (all ViewModels):
  - `using GalaSoft.MvvmLight` → `using CommunityToolkit.Mvvm.ComponentModel`
  - `using GalaSoft.MvvmLight.Command` → `using CommunityToolkit.Mvvm.Input`
  - `using GalaSoft.MvvmLight.Messaging` → `using CommunityToolkit.Mvvm.Messaging`
  - `: ViewModelBase` → `: ObservableObject`
  - `Set(() => this.Property, ref field, value)` → `SetProperty(ref field, value)`
  - `RaisePropertyChanged(() => this.Property)` → `OnPropertyChanged(nameof(Property))`
  - `this.MessengerInstance.Register/Send` → `WeakReferenceMessenger.Default.Register/Send`
- **RelayCommand migration:** Same class names, just different namespace — `CommunityToolkit.Mvvm.Input`
- **PropertyObserver replacement:**
  - Create `Utilities/PropertyObserver.cs` — small in-tree implementation (~50 lines) that watches `INotifyPropertyChanged` and routes property-specific callbacks
  - Update `SettingsWatcherBase` and `MainWindow.xaml.cs` to use the new implementation
- **IsInDesignMode replacement:** Use `DesignerProperties.GetIsInDesignMode(new DependencyObject())`
- Run all Phase A tests to verify behavioral parity

**At this point, the application compiles and runs with:** All ViewModel logic, messaging, and commands working. **Temporarily broken/changed:** UI may have issues if MahApps is not yet addressed (Section 8).

**Dependencies**

- Section 6 (SDK-style project must exist)

**Complexity**

- L (3-5 days)

**Key Risks**

- **Messenger signature change is the biggest risk.** MvvmLight: `Register<T>(recipient, Action<T>)`. CommunityToolkit: `Register<T>(recipient, MessageHandler<object, T>)` where handler is `(object recipient, T message) => ...`. There are ~20+ registrations across the codebase.
- **Weak references:** CommunityToolkit uses weak references by default. If a recipient gets GC'd, handlers silently stop. All recipients are held by strong references (ViewModels in DI, MainWindow by app) — but verify.
- **`Set()` method:** MvvmLight's `Set(() => this.Property, ref field, value)` uses a lambda. CommunityToolkit's `SetProperty(ref field, value, [CallerMemberName])` uses caller name. Must be called from the property setter. Some `Set` calls in `XmlUtilityViewModel` are in non-setter methods — verify each call site.

---

## 8. Replace MahApps.Metro with Custom Modern WPF UI

**Summary**

Remove MahApps.Metro entirely and replace with a custom, clean, modern WPF UI using `WindowChrome` for the borderless window, custom-styled controls, and the existing theme infrastructure. This is the UI modernization opportunity — a clean, minimal design that preserves all functionality while looking contemporary.

**Deliverables**

- Remove MahApps.Metro package reference
- **MainWindow.xaml rewrite:**
  - `MahApps:MetroWindow` → standard `Window` with `WindowChrome` for custom title bar
  - `MahApps:WindowCommands` → custom toolbar `StackPanel` in title bar area
  - `MahApps:FlyoutsControl` / `MahApps:Flyout` → custom sliding panel (`Border` with `TranslateTransform` animation or `Popup`)
  - Preserve all button commands, keyboard bindings, VisualStateManager, drag-drop, status bar
- **MainWindow.xaml.cs:**
  - `: MetroWindow` → `: Window`
  - Implement manual window position save/restore (replaces `SaveWindowPosition="True"`)
  - Remove `GlowBrush` — replace with `BorderBrush` or drop shadow
- **App.xaml:**
  - Remove MahApps resource dictionary references (`Colors.xaml`, `Fonts.xaml`, `Controls.xaml`)
  - Add custom base styles resource dictionary
- **Custom theme dictionaries:**
  - Update `ThemeColors.xaml`, `DarkThemeColors.xaml`, `SolarizedDarkThemeColors.xaml` to define all brush resources previously from MahApps (`AccentColor`, `AccentColorBrush`, `BlackBrush`, `WhiteBrush`, `Gray2Brush`, `Gray4Brush`, `LabelTextBrush`, `FlyoutBackgroundBrush`, `FlyoutForegroundBrush`)
  - Create `BaseStyles.xaml` for base control styles (Button, TextBox, ComboBox, ToggleButton, CheckBox, ScrollViewer, Slider)
- **SettingsView.xaml:** Replace `MahApps:ToggleSwitch` → styled `ToggleButton` or `CheckBox`
- **ApplicationThemeManager:** Replace index-based dictionary removal (`RemoveAt(5)`) with tag-based approach (custom key to identify theme dictionary)
- Add `WindowLeft`, `WindowTop`, `WindowWidth`, `WindowHeight`, `WindowState` properties to `Settings`
- Run all Phase A tests

**At this point, the application compiles and runs with:** All core features working. **Temporarily broken/changed:** Visual appearance is different (modern custom UI instead of MahApps Metro).

**Dependencies**

- Section 7 (MvvmLight must be replaced first)

**Complexity**

- XL (5+ days)

**Key Risks**

- **Highest-effort and highest-risk section.** MahApps provides extensive implicit styles affecting every control. Removing it will cause visual regressions. Plan for iterative visual testing.
- **`WindowChrome` quirks:** Close/minimize/maximize buttons need manual implementation. Hit-testing for title bar drag requires `WindowChrome.IsHitTestVisibleInChrome` on interactive elements.
- **`AccentColor` and `AccentColorBrush`** used extensively throughout — all `{DynamicResource ...}` references must resolve in custom theme dictionaries.

---

## 9. Upgrade AvalonEdit and Microsoft.Xaml.Behaviors.Wpf

**Summary**

Upgrade AvalonEdit from 4.4.x to 6.3.x and Microsoft.Xaml.Behaviors.Wpf from 1.1.19 to latest. AvalonEdit 6.x has API compatibility with 4.x for the features used in eXeMeL, but there may be minor breaking changes.

**Deliverables**

- Update `<PackageReference Include="AvalonEdit" Version="6.3.0.90" />` (or latest 6.x)
- Update `<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.77" />` (or latest 1.x)
- Verify AvalonEdit API compatibility:
  - `TextDocument` and `Document.Text`
  - `TextEditor` with `SyntaxHighlighting`, `Document`, `ShowLineNumbers`, `WordWrap` bindings
  - `TextEditorOptions` (`EnableHyperlinks`, `EnableEmailHyperlinks`, `AllowScrollBelowDocument`)
  - `FoldingManager.Install()` / `Uninstall()` / `XmlFoldingStrategy`
  - `HighlightingLoader.Load()` with `.xshd` resources
  - `TextViewPosition`, `CaretOffset`, `Select`, `ScrollTo`
  - Context menu commands
- Verify `.xshd` format compatibility (AvalonEdit 6.x uses same schema)
- Verify `AllSelectionColorizer` (custom `DocumentColorizingTransformer`) works with new API
- Run all Phase A tests
- Manual test: paste XML, verify syntax highlighting with all 5 schemes, verify folding

**Dependencies**

- Section 6 (SDK-style project must exist)
- Can run in parallel with Section 7

**Complexity**

- S (< 1 day)

**Key Risks**

- AvalonEdit 6.x may have removed some obsolete APIs. Verify `XmlFoldingStrategy` is still in `ICSharpCode.AvalonEdit.Folding`.
- `AllSelectionColorizer` extends `DocumentColorizingTransformer` — check if `ColorizeLine` API changed.

---

## 10. Modernize Settings Storage (Registry to Local File)

**Summary**

Replace Windows Registry-based settings storage with a local JSON file in `%LOCALAPPDATA%\eXeMeL\`. Implement automatic migration from Registry on first run. Switch from `DataContractJsonSerializer` to `System.Text.Json`.

**Deliverables**

- New `SettingsIO` implementation:
  - Settings file: `%LOCALAPPDATA%\eXeMeL\settings.json`
  - Save: serialize with `System.Text.Json.JsonSerializer` (WriteIndented)
  - Load: deserialize from file, fallback to defaults if missing/corrupt
  - Add `[JsonPropertyName]` attributes to match existing property names
- Create `SettingsMigrator`:
  - On first run, check if `settings.json` exists
  - If not, check Registry at `HKCU\Software\eXeMeL\Settings`
  - If registry exists, read with `DataContractJsonSerializer`, re-serialize with `System.Text.Json`, write to new file
  - Do NOT delete registry key (leave as backup)
- Keep `RegistryAccess` for migration only, mark `[Obsolete]`
- Update `MainViewModel` to call migration check before loading settings
- Rewrite `ApplicationVersionControl.GetPublishedVersion()` to use `Assembly.GetExecutingAssembly().GetName().Version`
- Move `LastLaunchedVersion` from registry to settings file
- Add window position properties to `Settings` (from Section 8)
- Run Phase A settings tests against both old and new formats
- Verify round-trip: old registry JSON → new file JSON → `Settings` with correct values

**Dependencies**

- Section 7 (CommunityToolkit.Mvvm must be in place)
- Section 2 (settings tests must exist)

**Complexity**

- M (1-3 days)

**Key Risks**

- **JSON format differences:** `DataContractJsonSerializer` serializes enums as integers; `System.Text.Json` as strings by default. Migration reader must use `DataContractJsonSerializer` to parse old format. Use `JsonStringEnumConverter` for new format.
- **Non-serialized properties:** `EditorBrush`, `ElementBrush`, etc. are computed, not `[DataMember]`. Add `[JsonIgnore]` to prevent `System.Text.Json` from serializing them.
- **First-run detection:** No registry + no file = fresh install (defaults). Registry only = migrate. File only = load file. Both = prefer file.

---

## 11. Rework App Startup (ClickOnce to Command-Line Args)

**Summary**

Replace ClickOnce `ActivationArguments` startup logic with standard command-line argument handling. Clean up remaining ClickOnce artifacts.

**Deliverables**

- Rewrite `App.xaml.cs` `OnStartup`:
  - Replace `ActivationArguments` with `e.Args.Length > 0 ? e.Args[0] : null`
  - Remove all commented-out ClickOnce code
- Verify `MainWindow_Loaded` still works: file path arg opens file, no args triggers clipboard refresh
- Delete ClickOnce artifacts:
  - All `.pfx` key files
  - `Properties/app.manifest` (preserve DPI settings if present — move to csproj as `<DpiAwareness>PerMonitorV2</DpiAwareness>`)
- Verify `ApplicationVersionControl.CurrentVersionIsDifferentFromLastRunVersion()` works with assembly version
- Remove `using System.Web` from `EditorViewModel.cs` (unused — `System.Net.WebUtility` is used in cleaners instead)

**Dependencies**

- Section 6 (SDK-style project)
- Section 10 (settings migration for version tracking)

**Complexity**

- S (< 1 day)

**Key Risks**

- `System.Web` is .NET Framework-only. Verify no code uses `HttpUtility` — `EncodedXmlExtractor` uses `WebUtility.HtmlDecode` from `System.Net`, which is fine.
- `Properties/app.manifest` may contain DPI awareness settings. Check before deleting.

---

## 12. Configure Single-File Self-Contained Publish

**Summary**

Set up publish configuration to produce a single self-contained `.exe`. Configure trimming cautiously and test the published output.

**Deliverables**

- Add publish profile `Properties/PublishProfiles/SingleFileRelease.pubxml`:
  - `PublishSingleFile=true`, `SelfContained=true`, `RuntimeIdentifier=win-x64`
  - `IncludeNativeLibrariesForSelfExtract=true`, `EnableCompressionInSingleFile=true`
- Add `<DebugType>embedded</DebugType>` for Release builds
- **Do NOT enable trimming** (`PublishTrimmed=false`) — WPF uses extensive reflection incompatible with IL trimming. Exe will be ~150-200 MB but reliable.
- Test publish: `dotnet publish -c Release -p:PublishProfile=SingleFileRelease`
- Verify single `eXeMeL.exe` in output
- Test on clean machine: launch with no args (clipboard), launch with file path, verify all features
- Document publish command in `CLAUDE.md`

**Dependencies**

- All prior sections (6-11) must be complete

**Complexity**

- S (< 1 day)

**Key Risks**

- Single-file WPF on .NET 9 is well-supported but exe will be large (~150-200 MB self-contained). Acceptable for desktop utility.
- `Assembly.GetExecutingAssembly().Location` returns empty in single-file apps. Verify no code depends on this.
- Embedded resources via `GetManifestResourceStream()` work correctly in single-file mode.

---

## 13. Final Regression Testing and Cleanup

**Summary**

Run full test suite, perform manual end-to-end testing of every feature, clean up dead code, update documentation, and prepare for release.

**Deliverables**

- Run all Phase A tests — 100% pass rate
- **Manual testing checklist:**
  - [ ] Launch with no args — clipboard XML read, cleaned, displayed
  - [ ] Launch with file path arg — file opened
  - [ ] Paste URL-encoded XML — decoded and formatted
  - [ ] Paste Visual Studio debug output — escaped quotes cleaned
  - [ ] Paste XML fragment — `<AddedRoot>` wrapper applied
  - [ ] F5 (Refresh) — reads clipboard
  - [ ] Ctrl+S (Save) — saves/prompts SaveFileDialog
  - [ ] Ctrl+O (Open) — opens via OpenFileDialog
  - [ ] Ctrl+F (Find) — focuses find control
  - [ ] F2 (Toggle mode) — Editor ↔ XPath Utility
  - [ ] XPath: type expression, elements highlighted
  - [ ] XPath: right-click → Copy XPath from root
  - [ ] XPath: right-click → Select start element
  - [ ] XPath: Collapse all other / Expand all child
  - [ ] Drag-drop .xml file — opens
  - [ ] Snapshots: create, navigate, labels correct
  - [ ] Right-click → Delve into Decoded XML
  - [ ] Settings: line numbers, word wrap, font size, font family
  - [ ] Settings: all 5 syntax highlighting schemes
  - [ ] Settings: all 3 application themes
  - [ ] Alt+1-9/- for folding, Alt+Shift+1-9/- for unfolding
  - [ ] Window position/size persists across sessions
  - [ ] View Change Log button works
  - [ ] Selection highlighting (other instances)
  - [ ] Status bar messages
- Clean up dead code: remove commented blocks, MvvmLight/MahApps references in comments
- Update `CLAUDE.md`: build instructions (`dotnet build`/`dotnet publish`), CommunityToolkit.Mvvm architecture, new settings location, command-line args, remove ClickOnce/packages.config/.NET 4.5 references
- Update test project to target `net9.0-windows`
- Verify published single-file exe on clean Windows machine

**Dependencies**

- All prior sections (1-12)

**Complexity**

- M (1-3 days)

**Key Risks**

- Manual testing is irreplaceable for UI work. Automated tests cover ViewModel logic but visual appearance, keyboard shortcuts, drag-drop, and clipboard must be manually verified.
- Clipboard access on locked/RDP machines can fail silently. Test in both local and remote desktop scenarios.
- Settings migration edge cases: corrupted registry, missing key, old-format JSON, fresh install.

---

## Dependency Graph

```
Phase A (parallel):
  1 ─┐
  2 ─┤──> 4
  3 ─┘

Phase B (parallel with Phase A):
  5

Phase C (sequential with checkpoints):
  5 ──> 6 ──> 7 ──> 8
              │
              └──> 9 (parallel with 7)
              │
              └──> 10 ──> 11
                           │
                    12 <───┘
                     │
                    13
```

## Total Estimated Effort

| Phase | Sections | Estimate |
|-------|----------|----------|
| A: Test Foundation | 1, 2, 3, 4 | 5-8 days |
| B: Tech Evaluation | 5 | 1-3 days |
| C: Migration | 6, 7, 8, 9, 10, 11, 12, 13 | 12-20 days |
| **Total** | | **18-31 days** |

The critical path runs through Sections 5 → 6 → 7 → 8 → 10 → 11 → 12 → 13. Section 8 (MahApps replacement) is the single largest risk and effort item.
