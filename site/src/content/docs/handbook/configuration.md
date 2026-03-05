---
title: Configuration
description: Backends, data storage, and troubleshooting.
sidebar:
  order: 3
---

## Data storage

All data is stored locally on your machine:

| Data | Location |
|------|----------|
| Sessions | `%LOCALAPPDATA%\InControl\sessions\` |
| Logs | `%LOCALAPPDATA%\InControl\logs\` |
| Cache | `%LOCALAPPDATA%\InControl\cache\` |
| Exports | `%USERPROFILE%\Documents\InControl\exports\` |

## Target hardware

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| GPU | RTX 3060 (8GB) | RTX 4080/5080 (16GB) |
| RAM | 16GB | 32GB |
| OS | Windows 10 1809+ | Windows 11 |
| .NET | 9.0 | 9.0 |

## Troubleshooting

**App won't start:**
- Check that .NET 9.0 Runtime is installed: `dotnet --list-runtimes`

**No models available:**
- Ensure Ollama is running: `ollama serve`
- Pull a model: `ollama pull llama3.2`

**GPU not detected:**
- Update NVIDIA drivers to the latest version
- Check CUDA toolkit installation

For more details, see [TROUBLESHOOTING.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/docs/TROUBLESHOOTING.md).
