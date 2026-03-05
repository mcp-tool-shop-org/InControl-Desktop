---
title: Architecture
description: Layered design and tech stack.
sidebar:
  order: 2
---

InControl follows a clean, layered architecture with strict one-way dependencies.

## Layer diagram

```
InControl.App (WinUI 3)           UI Layer
InControl.ViewModels               Presentation
InControl.Services                 Business Logic
InControl.Inference                LLM Backends
InControl.Core                     Shared Types
```

## Tech stack

| Layer | Technology |
|-------|------------|
| UI Framework | WinUI 3 (Windows App SDK 1.6) |
| Architecture | MVVM with CommunityToolkit.Mvvm |
| LLM Integration | OllamaSharp, Microsoft.Extensions.AI |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Configuration | Microsoft.Extensions.Configuration |
| Logging | Microsoft.Extensions.Logging + Serilog |

## Key design decisions

- **Privacy-first**: all conversations stay local, no cloud sync
- **Multi-backend**: Ollama, llama.cpp, or custom backends via the Inference abstraction
- **Native Windows**: WinUI 3 with Fluent Design, not Electron
- **Markdown rendering**: rich text, code blocks, and syntax highlighting in responses
