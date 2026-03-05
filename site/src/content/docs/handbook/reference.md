---
title: Reference
description: Architecture, NuGet packages, tech stack, and data storage for InControl Desktop.
sidebar:
  order: 3
---

## Architecture

InControl follows a clean, layered architecture:

```
+-------------------------------------------+
|         InControl.App (WinUI 3)           |  UI Layer
+-------------------------------------------+
|         InControl.ViewModels              |  Presentation
+-------------------------------------------+
|         InControl.Services                |  Business Logic
+-------------------------------------------+
|         InControl.Inference               |  LLM Backends
+-------------------------------------------+
|         InControl.Core                    |  Shared Types
+-------------------------------------------+
```

See [ARCHITECTURE.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/docs/ARCHITECTURE.md) for detailed design documentation.

## NuGet packages

| Package | Description |
|---------|-------------|
| **InControl.Core** | Domain models, conversation types, and shared abstractions for local AI chat applications |
| **InControl.Inference** | LLM backend abstraction layer with streaming chat, model management, and health checks. Includes Ollama implementation |

## Tech stack

| Layer | Technology |
|-------|------------|
| UI Framework | WinUI 3 (Windows App SDK 1.6) |
| Architecture | MVVM with CommunityToolkit.Mvvm |
| LLM Integration | OllamaSharp, Microsoft.Extensions.AI |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Configuration | Microsoft.Extensions.Configuration |
| Logging | Microsoft.Extensions.Logging + Serilog |

## Data storage

All data is stored locally:

| Data | Location |
|------|----------|
| Sessions | `%LOCALAPPDATA%\InControl\sessions\` |
| Logs | `%LOCALAPPDATA%\InControl\logs\` |
| Cache | `%LOCALAPPDATA%\InControl\cache\` |
| Exports | `%USERPROFILE%\Documents\InControl\exports\` |

See [PRIVACY.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/docs/PRIVACY.md) for complete data handling documentation.

## Security and data scope

InControl Desktop is a local-first WinUI 3 desktop application for private LLM chat.

- **Data accessed:** Local Ollama API (localhost), chat history in local storage, model configuration files
- **Data NOT accessed:** No cloud sync, no telemetry, no analytics. All inference runs locally via Ollama
- **Permissions:** Localhost network (Ollama API), file system for chat history. MSIX sandboxed
