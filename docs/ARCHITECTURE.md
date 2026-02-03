# Volt Architecture

## Overview

Volt is a Windows desktop application for local LLM chat, built with WinUI 3 and designed for RTX GPUs. This document describes the architectural decisions and layer responsibilities.

## Design Principles

1. **Separation of Concerns** - Each layer has a single responsibility
2. **Dependency Inversion** - Depend on abstractions, not implementations
3. **Testability** - Core logic is unit-testable without UI
4. **Extensibility** - New LLM backends can be added without changing upper layers
5. **Execution Boundaries** - All side effects go through controlled interfaces
6. **Deterministic State** - Immutable models with explicit transitions

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
│  - IFileStore / FileStore (path boundaries)                 │
│  - IHealthService / HealthService                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Volt.Inference                          │
│  LLM Backend Abstraction                                    │
│  - IInferenceClient (abstraction)                           │
│  - FakeInferenceClient (testing)                            │
│  - OllamaInferenceClient (future)                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Volt.Core                            │
│  Shared Types & Contracts                                   │
│  - Message, Conversation, AppState (immutable)              │
│  - VoltError, Result<T> (error handling)                    │
│  - BuildInfo, TrustReport (trust signals)                   │
│  - StateSerializer (deterministic JSON)                     │
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
│   │   ├── Storage/              # IFileStore, FileStore
│   │   ├── Health/               # IHealthCheck, HealthService
│   │   └── Volt.Services.csproj
│   │
│   ├── Volt.Inference/           # LLM Backends
│   │   ├── Interfaces/
│   │   ├── Fakes/                # FakeInferenceClient
│   │   └── Volt.Inference.csproj
│   │
│   └── Volt.Core/                # Shared Types
│       ├── Models/               # Message, Conversation, etc.
│       ├── Errors/               # VoltError, Result<T>
│       ├── State/                # AppState, StateSerializer
│       ├── Trust/                # BuildInfo, TrustReport
│       └── Volt.Core.csproj
│
├── tests/
│   ├── Volt.Core.Tests/          # 98 tests
│   ├── Volt.Services.Tests/      # 68 tests
│   └── Volt.Inference.Tests/     # 19 tests
│
├── scripts/
│   ├── verify.ps1                # CI-like verification (Windows)
│   ├── verify.sh                 # CI-like verification (Unix)
│   ├── coverage.ps1              # Coverage collection (Windows)
│   └── coverage.sh               # Coverage collection (Unix)
│
├── docs/
│   ├── ARCHITECTURE.md           # This file
│   └── PHASE2_ACCEPTANCE.md      # Phase 2 gate status
│
├── Directory.Build.props         # Shared MSBuild properties
├── Directory.Packages.props      # Central package management
├── Volt.sln
├── LICENSE                       # MIT License
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
    string BackendName { get; }
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken ct = default);
    Task<ModelInfo?> GetModelAsync(string modelId, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamChatAsync(ChatRequest request, CancellationToken ct = default);
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken ct = default);
}
```

### IFileStore

Filesystem abstraction with path boundary enforcement:

```csharp
public interface IFileStore
{
    string AppDataPath { get; }
    bool IsPathAllowed(string path);
    Task<Result<string>> ReadTextAsync(string relativePath, CancellationToken ct = default);
    Task<Result> WriteTextAsync(string relativePath, string content, CancellationToken ct = default);
    Task<Result> DeleteAsync(string relativePath, CancellationToken ct = default);
    Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default);
}
```

### Result<T>

Monadic error handling pattern:

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T? Value { get; }
    public VoltError? Error { get; }

    public Result<TNew> Map<TNew>(Func<T, TNew> mapper);
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder);
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<VoltError, TResult> onFailure);
}
```

### IHealthCheck

Health monitoring probe interface:

```csharp
public interface IHealthCheck
{
    string Name { get; }
    string Category { get; }
    Task<HealthProbeResult> CheckAsync(CancellationToken ct = default);
}
```

## Error Handling Strategy

Phase 2 introduced structured error handling:

1. **VoltError** - Immutable error record with code, message, suggestions
2. **Result<T>** - Either success value or error (no exceptions for expected failures)
3. **ErrorCode** - 30+ categorized error codes
4. **ErrorSeverity** - Info, Warning, Error, Critical

Error flow:
- Volt.Inference returns `Result<T>` from operations
- Volt.Services uses `Map`/`Bind` for composition
- Volt.ViewModels uses `Match` for UI presentation
- Exceptions reserved for truly exceptional conditions

## State Management

Phase 2 introduced deterministic state:

1. **Immutable Records** - `Message`, `Conversation`, `AppState` are sealed records
2. **Factory Methods** - `With*` methods return new instances
3. **StateSerializer** - Deterministic JSON serialization
4. **Round-trip Tests** - All state types have serialization tests

```csharp
// Immutable state transitions
var newState = state
    .WithConversation(conversation)
    .WithActiveConversation(conversation.Id)
    .WithModelSelection(ModelSelectionState.WithModel("llama3.2"));
```

## Trust & Security

Phase 2 added trust infrastructure:

1. **BuildInfo** - Version, commit hash, build timestamp
2. **TrustReport** - Aggregated security and runtime info
3. **SecurityConfig** - Path boundaries, inference isolation
4. **TrustLevel** - Computed trust level (High/Medium/Low)

## Configuration

Configuration is loaded from multiple sources with precedence:

1. `appsettings.json` - Base configuration
2. `appsettings.Development.json` - Dev overrides
3. Environment variables - Runtime overrides
4. User settings file - User preferences

## Threading Model

- UI runs on the main thread (WinUI 3 STA)
- Inference operations run on background threads
- ViewModels marshal results back to UI thread via `DispatcherQueue`
- All async methods support `CancellationToken`

---

## Phase 2 Status

All Phase 2 gates have been implemented:

| Gate | Status |
|------|--------|
| Build & Test Integrity | ✅ |
| Test Coverage Floor | ✅ |
| Execution Boundary Enforcement | ✅ |
| Deterministic State & Persistence | ✅ |
| Health, Errors, and Failure Clarity | ✅ |
| Architecture Lock | ⏳ Awaiting sign-off |
| User-Visible Trust Signals | ✅ |

See `docs/PHASE2_ACCEPTANCE.md` for detailed gate status.

---

## Out-of-Scope for Phase 2

The following are explicitly not part of Phase 2:

- Multi-user support
- Cloud sync or backup
- Plugin/extension system
- Custom model training or fine-tuning
- Voice input/output
- Cross-platform support (Windows only)
- Web interface
- Mobile companion app
- Ollama installation/management (user responsibility)

## Extension Points

These interfaces allow future extension without architectural changes:

| Interface | Purpose | Example Extensions |
|-----------|---------|-------------------|
| `IInferenceClient` | Add new LLM backends | llama.cpp, vLLM, LocalAI |
| `IFileStore` | Add new storage backends | Cloud storage, encrypted storage |
| `IHealthCheck` | Add new health probes | GPU monitoring, disk space |
| Configuration | Add new settings sections | Theming, keyboard shortcuts |

## Non-Goals

These are explicitly not goals for Volt:

- Cross-platform support (Windows App SDK is Windows-only)
- Web interface (desktop-first design)
- Mobile companion app
- Model fine-tuning or training
- Multi-user or team features
- Cloud deployment

---

## Phase 3 Preview

Phase 3 will focus on:

1. Service implementations (ChatService, SettingsService)
2. Ollama integration (OllamaInferenceClient)
3. UI implementation (XAML views and bindings)
4. End-to-end conversation flow

Architecture freeze applies: no new projects without plan update.
