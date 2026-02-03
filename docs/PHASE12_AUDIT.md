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

## Commit 04: 2-Hour Soak Test

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Evidence
- Screen recording: `docs/phase12/recordings/soak-test.mp4`

---

## Commit 05: Crash Reporting + Recovery UX

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Screenshots
*(To be completed)*

---

## Commit 06: Button Coverage Tests

### What Changed
*(To be completed)*

### Test Evidence
*(To be completed)*

### Screenshots
*(To be completed)*

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
