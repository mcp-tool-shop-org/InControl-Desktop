# Changelog

All notable changes to InControl-Desktop will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

---

## [0.9.0-rc.1] - 2026-02-03

> **Release Candidate** - Pre-release for testing. Not recommended for production use.

### Highlights
- Full Ollama integration for local AI model management
- Complete UI framework with 15+ pages and controls
- Phase 12 release candidate preparation

### Added
- **Ollama Integration**: Direct connection to local Ollama instance
  - Live model list with metadata (size, family, parameters)
  - Pull models directly from Ollama library
  - Quick-pull buttons for popular models (llama3.2, mistral, codegemma)
  - Connection status indicator with version display
- **Personal Assistant System**: Local-first assistant with context memory
- **Conversation Management**: Multi-session support with sidebar navigation
- **Memory System**: Persistent context across sessions
- **Tool Execution Framework**: With approval controls and sandboxing
- **Offline-First Architecture**: Complete air-gap capability
- **Update Management**: Operator-controlled update policies
- **Audit Logging**: Full network activity tracking
- **Command Palette**: Ctrl+K quick access to all functions
- **Inspector Panel**: Real-time inference statistics
- **Support Bundle**: One-click diagnostic export

### Changed
- Welcome text updated from "RTX GPU" to "Ollama-powered"
- Model Manager redesigned for Ollama-native workflow
- Improved theme resource organization

### Fixed
- Resource dictionary not merged causing page crashes
- DateTime nullable handling in model info display

### Security
- All network access disabled by default (OfflineOnly mode)
- Explicit operator approval required for internet connectivity
- Per-endpoint permission rules
- Complete audit trail for network requests

### Known Issues
- Branch protection requires manual GitHub admin configuration
- MSIX signing requires certificate setup for production

---

## [0.1.0] - 2026-01-15 - Initial Release

### Added
- Core chat interface with streaming responses
- Ollama integration for local LLM inference
- Session management and persistence
- Theme support (Light, Dark, System)
- Tray icon with minimize to tray option
- Basic settings management

### Technical
- Built on .NET 9.0 and WinUI 3
- MVVM architecture with CommunityToolkit
- Local SQLite storage for conversations
- RTX GPU acceleration support

---

## Version Numbering

- **MAJOR**: Breaking changes to user data or settings
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, no feature changes

## Upgrade Notes

When upgrading between versions:

1. **Backup your data** before upgrading (Settings → Export)
2. Review the changelog for any migration notes
3. Check the [Release Notes](./docs/RELEASE_NOTES.md) for version-specific details

## Reporting Issues

Found a bug? Please report it:
1. Check if the issue already exists in [Issues](../../issues)
2. Create a new issue with steps to reproduce
3. Include your version number and system info
