# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 0.9.x   | :white_check_mark: |
| < 0.9   | :x:                |

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability in InControl, please report it responsibly.

### How to Report

1. **Do NOT** create a public GitHub issue for security vulnerabilities
2. Email security concerns to: **security@mcp-tool-shop.dev** (or create a private security advisory)
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Any suggested fixes (optional)

### What to Expect

- **Acknowledgment**: Within 48 hours of your report
- **Initial Assessment**: Within 7 days
- **Resolution Timeline**: Depends on severity
  - Critical: 24-72 hours
  - High: 1-2 weeks
  - Medium: 2-4 weeks
  - Low: Next release cycle

### Scope

The following are in scope:
- InControl Desktop application
- Local data storage security
- Model loading/execution security
- Extension/plugin security
- Network communications (Ollama API)

### Out of Scope

- Vulnerabilities in Ollama itself (report to Ollama project)
- Social engineering attacks
- Physical access attacks
- Denial of service attacks

## Security Best Practices for Users

1. **Keep InControl Updated**: Always run the latest version
2. **Verify Downloads**: Check MSIX signatures before installing
3. **Model Sources**: Only load models from trusted sources
4. **Extensions**: Only install extensions you trust
5. **Offline Mode**: Use offline mode when handling sensitive data

## Acknowledgments

We appreciate security researchers who help keep InControl safe. Contributors who report valid vulnerabilities will be acknowledged (with permission) in our release notes.
