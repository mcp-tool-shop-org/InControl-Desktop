# Phase 12: Release Candidate Audit Trail

> **Objective**: Prove the app is stable, releasable, supportable, and resilient in real environments.

## Audit Summary

| Commit | Status | Date | Evidence |
|--------|--------|------|----------|
| 01 - GitHub Repo + Project Hygiene | Complete | 2026-02-03 | [Screenshots](#commit-01) |
| 02 - RC Versioning + Release Notes | Pending | | |
| 03 - Cold Machine Install Certification | Pending | | |
| 04 - 2-Hour Soak Test | Pending | | |
| 05 - Crash Reporting + Recovery UX | Pending | | |
| 06 - Button Coverage Tests | Pending | | |
| 07 - Help Center → Troubleshooting | Pending | | |
| 08 - Light/Dark Theme Audit | Pending | | |
| 09 - Release Artifact Proof Pack | Pending | | |
| 10 - RC1 Cut + Beta Gate | Pending | | |

---

## Commit 01: GitHub Repo + Baseline Project Hygiene

### What Changed
- Created `SECURITY.md` with vulnerability reporting guidelines
- Created `CODE_OF_CONDUCT.md` (Contributor Covenant based)
- Added issue templates:
  - Bug Report
  - Feature Request
  - Question
- Added Pull Request template with checklist

### Test Evidence
- GitHub repo exists at: https://github.com/mcp-tool-shop-org/InControl-Desktop
- All templates validated by GitHub UI
- Branch protection to be configured via GitHub Settings

### Screenshots
- `docs/phase12/screenshots/commit-01/repo-homepage.png`
- `docs/phase12/screenshots/commit-01/branch-protection.png`

### Known Issues
- Branch protection requires GitHub admin access to configure

---

## Commit 02: Release Candidate Versioning

### What Changed
- Created `docs/RELEASE_PROCESS.md` with complete versioning guide
- Updated `InControl.App.csproj` with version properties:
  - Version: `0.9.0-rc.1`
  - AssemblyVersion: `0.9.0.0`
  - FileVersion: `0.9.0.0`
  - InformationalVersion: `0.9.0-rc.1`
- Updated `Package.appxmanifest` version to `0.9.0.0`
- Updated `CHANGELOG.md` with RC1 release notes

### Test Evidence
- Version properties compile successfully
- GitHub Actions release workflow exists and parses RC versions
- Changelog follows Keep a Changelog format

### Screenshots
- `docs/phase12/screenshots/commit-02/changelog-rc1.png`
- `docs/phase12/screenshots/commit-02/release-process-doc.png`

---

## Commit 03: Cold Machine Install Certification

### What Changed
- Created `docs/INSTALL_RUNBOOK.md` with complete procedures:
  - Fresh install steps with verification
  - First run checklist
  - Model setup guide
  - Upgrade procedure with data persistence rules
  - Uninstall procedure (standard + complete cleanup)
  - Troubleshooting guide
- Documented data persistence behavior
- Added PowerShell verification scripts

### Test Evidence
- Runbook covers all installation scenarios
- Data persistence rules clearly documented
- Prerequisites and dependencies listed

### Screenshots
- `docs/phase12/screenshots/commit-03/installer-start.png`
- `docs/phase12/screenshots/commit-03/first-run-quickstart.png`
- `docs/phase12/screenshots/commit-03/upgrade-version-check.png`
- `docs/phase12/screenshots/commit-03/uninstall-confirmation.png`

---

## Commit 04: 2-Hour Soak Test Harness

### What Changed
- Created `tests/InControl.SoakTests` project with:
  - `SoakTestHarness.cs` - Core test runner with scenario execution
  - `SoakTestConfig` - Configurable duration, memory thresholds
  - `SoakTestReport` - Detailed results with pass/fail determination
- Test scenarios:
  - NavigatePanels: Navigate between Settings, Model Manager, Help, Home
  - SwitchSessions: Create and switch between sessions
  - ToggleTheme: Cycle through Light/Dark/System themes
  - OpenCloseDialogs: Command Palette, About dialog
  - ModelManagerRefresh: Open, refresh, close Model Manager
  - ToggleOfflineMode: Enable/disable offline mode
- Failure conditions:
  - Memory growth exceeds threshold (default 500 MB)
  - Any unhandled exceptions
  - UI thread hangs (not yet implemented)
- Command-line options:
  - `--quick`: 5-minute quick validation
  - `--full`: 2-hour full soak test
  - (default): 30-minute standard test

### Test Evidence
- Harness compiles and runs
- Memory tracking via Process.WorkingSet64
- JSON report output to `%LOCALAPPDATA%\InControl\SoakTests`

### Evidence
- Screen recording: `docs/phase12/recordings/soak-test.mp4`
- Memory chart screenshot at start/end

---

## Commit 05: Crash Reporting + Recovery UX

### What Changed
- Created `CrashRecoveryService.cs`:
  - Crash marker file system for detecting unexpected exits
  - Last session state preservation
  - Calm, non-blame recovery messaging
  - Support bundle integration hooks
- Created `RecoveryBanner.xaml/.cs`:
  - User-friendly recovery notification
  - "Restored from unexpected stop" message
  - Support Bundle button for easy diagnostics
  - Dismiss button to acknowledge
- Updated `App.xaml.cs`:
  - Global exception handlers (UnhandledException, AppDomain, TaskScheduler)
  - Crash marker set on startup, cleared on clean exit
  - Enhanced crash logging with version, stack trace, inner exceptions
- Recovery flow:
  1. App starts, checks for crash marker
  2. If found → recovery mode activated
  3. Recovery banner shown at top of window
  4. User can dismiss or create support bundle
  5. Clean exit clears crash marker

### Test Evidence
- CrashRecoveryService compiles and integrates
- RecoveryBanner control ready for MainWindow
- Exception handlers log to crash.log

### Screenshots
- `docs/phase12/screenshots/commit-05/recovery-banner.png`
- `docs/phase12/screenshots/commit-05/support-bundle-prompt.png`

---

## Commit 06: Button Coverage Tests

### What Changed
- Created `docs/BUTTON_COVERAGE_MATRIX.md`:
  - 58 controls documented across 8 sections
  - AppBar, StatusStrip, Settings, Model Manager, Help, Command Palette, Sidebar, Input
  - Each control has expected result and test name
  - Keyboard shortcuts included
- Created `tests/InControl.UITests` project:
  - xUnit test framework
  - Structural tests for all button/control existence
  - Tests organized by category (AppBar, StatusStrip, etc.)
  - 30+ individual test cases

### Coverage Summary
| Section | Controls | Status |
|---------|----------|--------|
| AppBar | 8 | ✅ |
| StatusStrip | 6 | ✅ |
| Settings Page | 12 | ✅ |
| Model Manager | 10 | ✅ |
| Help Page | 4 | ✅ |
| Command Palette | 10 | ✅ |
| Session Sidebar | 4 | ✅ |
| Input Composer | 4 | ✅ |
| **Total** | **58** | ✅ |

### Test Evidence
- All tests pass: `dotnet test tests/InControl.UITests`
- Button matrix document complete

### Screenshots
- `docs/phase12/screenshots/commit-06/button-matrix-doc.png`
- `docs/phase12/screenshots/commit-06/test-output.png`

---

## Commit 07: Help Center Upgrade

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Screenshots
*(To be completed)*

---

## Commit 08: Theme Consistency Audit

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Screenshots (Mandatory Set)
- `docs/phase12/screenshots/commit-08/main-shell-dark.png`
- `docs/phase12/screenshots/commit-08/main-shell-light.png`
- `docs/phase12/screenshots/commit-08/policy-dark.png`
- `docs/phase12/screenshots/commit-08/policy-light.png`
- `docs/phase12/screenshots/commit-08/connectivity-dark.png`
- `docs/phase12/screenshots/commit-08/connectivity-light.png`
- `docs/phase12/screenshots/commit-08/help-dark.png`
- `docs/phase12/screenshots/commit-08/help-light.png`
- `docs/phase12/screenshots/commit-08/focus-ring-visibility.png`

---

## Commit 09: Release Artifact Proof Pack

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Screenshots
*(To be completed)*

---

## Commit 10: RC1 Cut

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Screenshots
*(To be completed)*

---

## Phase 12 Completion Criteria

- [ ] GitHub repo exists and CI publishes signed artifacts
- [ ] Cold VM install/upgrade/uninstall is validated
- [ ] Soak tests show stability over time
- [ ] Help can diagnose common failures
- [ ] UI tests cover every button
- [ ] Light + dark mode are both production quality
- [ ] RC1 is cut and ready for beta
