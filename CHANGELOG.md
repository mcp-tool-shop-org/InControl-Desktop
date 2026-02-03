# Changelog

All notable changes to InControl-Desktop will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Personal assistant system with local-first design
- Conversation management with multi-session support
- Memory system for persistent context
- Tool execution framework with approval controls
- Offline-first connectivity architecture
- Update management with operator control
- Audit logging for all network activity

### Security
- All network access disabled by default (OfflineOnly mode)
- Explicit operator approval required for internet connectivity
- Per-endpoint permission rules
- Complete audit trail for network requests

## [0.1.0] - Initial Release

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
