# Volt Architecture

## Overview

Volt is a Windows desktop application for local LLM chat, built with WinUI 3 and designed for RTX GPUs. This document describes the architectural decisions and layer responsibilities.

## Design Principles

1. **Separation of Concerns** - Each layer has a single responsibility
2. **Dependency Inversion** - Depend on abstractions, not implementations
3. **Testability** - Core logic is unit-testable without UI
4. **Extensibility** - New LLM backends can be added without changing upper layers

## Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Volt.App                             │
│  WinUI 3 Application Host                                   │
│  - App.xaml / MainWindow                                    │
│  - DI Container Setup                                       │
│  - Navigation                                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Volt.ViewModels                         │
│  MVVM Presentation Layer                                    │
│  - ChatViewModel                                            │
│  - SettingsViewModel                                        │
│  - Commands and State                                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Volt.Services                          │
│  Business Logic & Orchestration                             │
│  - IChatService / ChatService                               │
│  - ISettingsService / SettingsService                       │
│  - IModelService / ModelService                             │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Volt.Inference                          │
│  LLM Backend Abstraction                                    │
│  - IInferenceClient (abstraction)                           │
│  - OllamaInferenceClient                                    │
│  - LlamaCppInferenceClient (future)                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Volt.Core                            │
│  Shared Types & Contracts                                   │
│  - Message, Conversation DTOs                               │
│  - Configuration Models                                     │
│  - Exception Types                                          │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
Volt/
├── src/
│   ├── Volt.App/                 # WinUI 3 Application
│   │   ├── App.xaml(.cs)
│   │   ├── MainWindow.xaml(.cs)
│   │   ├── Views/
│   │   ├── Controls/
│   │   └── Volt.App.csproj
│   │
│   ├── Volt.ViewModels/          # MVVM ViewModels
│   │   ├── ChatViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   └── Volt.ViewModels.csproj
│   │
│   ├── Volt.Services/            # Business Logic
│   │   ├── Interfaces/
│   │   ├── ChatService.cs
│   │   └── Volt.Services.csproj
│   │
│   ├── Volt.Inference/           # LLM Backends
│   │   ├── Interfaces/
│   │   ├── Ollama/
│   │   └── Volt.Inference.csproj
│   │
│   └── Volt.Core/                # Shared Types
│       ├── Models/
│       ├── Exceptions/
│       └── Volt.Core.csproj
│
├── tests/
│   ├── Volt.Core.Tests/
│   ├── Volt.Services.Tests/
│   └── Volt.Inference.Tests/
│
├── docs/
│   └── ARCHITECTURE.md
│
├── Directory.Build.props         # Shared MSBuild properties
├── Directory.Packages.props      # Central package management
├── Volt.sln
└── README.md
```

## Dependency Flow

```
Volt.App ──────► Volt.ViewModels
    │                 │
    │                 ▼
    │           Volt.Services
    │                 │
    │                 ▼
    └──────────► Volt.Inference
                      │
                      ▼
                 Volt.Core ◄────── (all projects reference)
```

## Key Abstractions

### IInferenceClient

The core abstraction for LLM communication:

```csharp
public interface IInferenceClient
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    IAsyncEnumerable<string> StreamChatAsync(
        ChatRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyList<ModelInfo>> ListModelsAsync(
        CancellationToken ct = default);
}
```

### IChatService

Orchestrates conversations with history management:

```csharp
public interface IChatService
{
    Task<Conversation> CreateConversationAsync();

    IAsyncEnumerable<string> SendMessageAsync(
        Guid conversationId,
        string message,
        CancellationToken ct = default);

    Task<IReadOnlyList<Conversation>> GetConversationsAsync();
}
```

## Configuration

Configuration is loaded from multiple sources with precedence:

1. `appsettings.json` - Base configuration
2. `appsettings.Development.json` - Dev overrides
3. Environment variables - Runtime overrides
4. User settings file - User preferences

```json
{
  "Inference": {
    "Backend": "Ollama",
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "DefaultModel": "llama3.2"
    }
  },
  "Logging": {
    "MinLevel": "Information",
    "FilePath": "%LOCALAPPDATA%/Volt/logs"
  }
}
```

## Error Handling Strategy

1. **Volt.Core** defines custom exception types
2. **Volt.Inference** wraps backend errors in `InferenceException`
3. **Volt.Services** handles retries and circuit breaking
4. **Volt.ViewModels** presents user-friendly error messages
5. **Volt.App** shows error UI (dialogs, toasts)

## Threading Model

- UI runs on the main thread (WinUI 3 STA)
- Inference operations run on background threads
- ViewModels marshal results back to UI thread via `DispatcherQueue`
- All async methods support `CancellationToken`

## Phase 1 Goals (Deterministic Layers)

Phase 1 focuses on infrastructure that doesn't require external dependencies:

1. ✅ Solution and project structure
2. ✅ Directory.Build.props / Directory.Packages.props
3. ⬜ Volt.Core - Models and exceptions
4. ⬜ Volt.Core - Configuration contracts
5. ⬜ Volt.Inference - IInferenceClient interface
6. ⬜ Volt.Services - Service interfaces
7. ⬜ Volt.ViewModels - Base ViewModel infrastructure
8. ⬜ Volt.App - DI container setup
9. ⬜ Logging infrastructure with Serilog
10. ⬜ Unit test project structure

No actual Ollama calls, no UI rendering - just the deterministic foundation.
