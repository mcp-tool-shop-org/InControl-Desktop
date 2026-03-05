---
title: Troubleshooting
description: Common issues and quick fixes for InControl Desktop.
sidebar:
  order: 4
---

For a full list of known issues, see [TROUBLESHOOTING.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/docs/TROUBLESHOOTING.md) in the repository.

## App won't start

- Check that .NET 9.0 Runtime is installed
- Run `dotnet --list-runtimes` to verify

## No models available

- Ensure Ollama is running: `ollama serve`
- Pull a model: `ollama pull llama3.2`
- Confirm Ollama is listening on `http://localhost:11434`

## GPU not detected

- Update NVIDIA drivers to the latest version
- Check CUDA toolkit installation
- Verify your GPU meets the minimum requirement (RTX 3060 with 8GB VRAM)

## Reporting issues

1. Check [TROUBLESHOOTING.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/docs/TROUBLESHOOTING.md) first
2. Use the "Copy Diagnostics" feature in the app
3. Open an issue at [GitHub Issues](https://github.com/mcp-tool-shop-org/InControl-Desktop/issues) with diagnostics info attached

## Getting help

- **Questions:** [Discussions](https://github.com/mcp-tool-shop-org/InControl-Desktop/discussions)
- **Bug reports:** [Issues](https://github.com/mcp-tool-shop-org/InControl-Desktop/issues)
- **Security:** [SECURITY.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/SECURITY.md)
