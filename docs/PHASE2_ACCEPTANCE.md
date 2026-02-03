# Volt — Phase 2 Acceptance Gates

**Phase 2 Theme: Execution integrity + user-visible trust**

Phase 1 proved Volt is real.
Phase 2 must prove Volt is safe to operate.

No new "big features."
Only things that make the system reliable, inspectable, and bounded.

---

## Gate Status Summary

| Gate | Name | Type | Status |
|------|------|------|--------|
| 1 | Build & Test Integrity | ENFORCED | ⚙️ IN PROGRESS |
| 2 | Test Coverage Floor | ENFORCED | ⛔ BLOCKED |
| 3 | Execution Boundary Enforcement | ENFORCED | ⛔ BLOCKED |
| 4 | Deterministic State & Persistence | ENFORCED | ⛔ BLOCKED |
| 5 | Health, Errors, and Failure Clarity | ENFORCED | ⛔ BLOCKED |
| 6 | Architecture Lock | HUMAN-VERIFIED | ⛔ BLOCKED |
| 7 | User-Visible Trust Signals | ENFORCED | ⛔ BLOCKED |

---

## Gate 1 — Build & Test Integrity (ENFORCED)

**Status:** ⚙️ IN PROGRESS

**Goal:** Any clone builds and tests cleanly, deterministically.

### Acceptance Criteria

- [x] `dotnet build` succeeds from repo root
- [x] `dotnet test` passes all test projects
- [ ] No warnings elevated to errors unless explicitly justified
- [x] No `bin/` or `obj/` artifacts committed
- [x] `.gitignore` present and effective

### Evidence Required

- CI-equivalent local run transcript
- Clean repo tree after build

### Current State

**Assessed: 2026-02-02**

✅ **Build succeeds** — All 8 projects compile (Core, Inference, Services, ViewModels, App, and 3 test projects)

✅ **Tests pass** — 20 tests across 3 test projects:
- Volt.Core.Tests: 16 passed
- Volt.Inference.Tests: 3 passed
- Volt.Services.Tests: 1 passed

⚠️ **Warning present** — MSB3277 WinRT.Runtime version conflict in Volt.App.csproj
- This is a Windows App SDK package version mismatch (2.1.0 vs 2.2.0)
- Non-blocking but should be resolved

✅ **No artifacts committed** — `.gitignore` properly excludes `bin/`, `obj/`

### Blocking Issues

1. Resolve WinRT.Runtime version conflict warning
2. Add CI workflow (GitHub Actions) for automated verification

### Why This Gate Exists

Volt cannot earn trust if it can't be rebuilt by a stranger.

---

## Gate 2 — Test Coverage Floor (ENFORCED)

**Status:** ⛔ BLOCKED

**Goal:** Core logic is meaningfully protected.

### Acceptance Criteria

- [x] Coverage collection enabled (Coverlet or equivalent)
- [ ] Minimum thresholds:
  - [ ] `Volt.Core` ≥ 80% (current: **32.85%**)
  - [ ] `Volt.Services` ≥ 70% (current: **0%**)
  - [ ] `Volt.Inference` ≥ 70% (current: **5.8%**)
- [ ] ViewModels excluded only if justified

### Explicitly NOT Allowed

- Fake coverage via trivial getters/setters
- Tests that assert only construction

### Evidence Required

- Coverage report artifact
- Failing build if thresholds drop

### Current State

**Assessed: 2026-02-02**

Coverage collection works (Coverlet via `--collect:"XPlat Code Coverage"`).

| Project | Line Coverage | Required | Gap |
|---------|--------------|----------|-----|
| Volt.Core | 32.85% | 80% | -47.15% |
| Volt.Services | 0% | 70% | -70% |
| Volt.Inference | 5.8% | 70% | -64.2% |

### Blocking Issues

1. **Volt.Core**: Add tests for `ChatRequest`, `ChatResponse`, configuration models
2. **Volt.Services**: No implementations exist — cannot test what doesn't exist
3. **Volt.Inference**: No implementations exist — only interfaces tested
4. **Threshold enforcement**: Add coverage threshold to build (via `runsettings` or `coverlet.runsettings.xml`)

### Why This Gate Exists

Phase 2 is where regressions start to matter.

---

## Gate 3 — Execution Boundary Enforcement (ENFORCED)

**Status:** ⛔ BLOCKED

**Goal:** Volt must not perform uncontrolled side effects.

### Acceptance Criteria

- [x] All external effects routed through explicit service interfaces
- [ ] All external effects routed through centralized execution points
- [ ] No direct file system writes outside approved paths
- [ ] No direct network calls outside declared adapters
- [x] Inference calls isolated behind abstractions

### Evidence Required

- Centralized service registry
- Tests proving boundary enforcement
- Static scan or guard tests if applicable

### Current State

**Assessed: 2026-02-02**

✅ **Interface architecture is sound:**
- `IChatService` — conversation orchestration
- `IConversationStorage` — file I/O abstraction
- `IInferenceClient` — network abstraction
- `ISettingsService` — configuration I/O
- `INavigationService` — UI navigation

⛔ **No implementations exist:**

All service registrations in `ServiceCollectionExtensions.cs` are **commented out**:
```csharp
// services.AddSingleton<IChatService, ChatService>();
// services.AddSingleton<ISettingsService, SettingsService>();
// services.AddSingleton<IConversationStorage, JsonConversationStorage>();
// services.AddSingleton<INavigationService, NavigationService>();
```

### Blocking Issues

1. Implement all service classes:
   - `ChatService`
   - `SettingsService`
   - `JsonConversationStorage`
   - `NavigationService`
   - `OllamaInferenceClient`
   - `InferenceClientFactory`
   - `OllamaModelManager`

2. Add boundary enforcement tests:
   - File writes only to `%LOCALAPPDATA%/Volt/`
   - Network calls only via `IInferenceClient`
   - No `System.Net.Http.HttpClient` usage outside Inference layer

### Why This Gate Exists

This is what separates "app" from "safe system."

---

## Gate 4 — Deterministic State & Persistence (ENFORCED)

**Status:** ⛔ BLOCKED

**Goal:** Volt state is predictable and inspectable.

### Acceptance Criteria

- [x] Single source of truth for:
  - [x] Conversations (sealed record)
  - [x] Messages (sealed record)
  - [ ] Model selection (not implemented)
- [x] State transitions are:
  - [x] Explicit (factory methods)
  - [ ] Serializable (not tested)
  - [ ] Replayable (not implemented)
- [x] No hidden mutable globals

### Evidence Required

- State model documentation
- Tests validating round-trip serialization
- Clear ownership of state mutations

### Current State

**Assessed: 2026-02-02**

✅ **Domain models are immutable:**
- `Conversation` — sealed record with `WithMessage()`, `WithTitle()` factory methods
- `Message` — sealed record with `User()`, `Assistant()`, `System()` factories
- All collections use `IReadOnlyList<T>`

⚠️ **ViewModel state duplication:**
- `ChatViewModel.Messages` is `ObservableCollection<MessageViewModel>` (mutable)
- Not synchronized with underlying `Conversation.Messages`
- Streaming requires mutable `MessageViewModel._contentBuilder`

⛔ **No persistence implementation:**
- `IConversationStorage` interface exists
- No `JsonConversationStorage` implementation
- No serialization tests

### Blocking Issues

1. Implement `JsonConversationStorage`
2. Add round-trip serialization tests for all domain models
3. Document state ownership: domain models are authoritative, ViewModels are projections
4. Add ViewModel-to-domain synchronization tests

### Why This Gate Exists

Debugging without state determinism is a dead end.

---

## Gate 5 — Health, Errors, and Failure Clarity (ENFORCED)

**Status:** ⛔ BLOCKED

**Goal:** Failure is visible and understandable.

### Acceptance Criteria

- [x] Health model exists
- [x] Errors are:
  - [x] Typed
  - [ ] Actionable (not tested)
  - [ ] User-safe (not verified)
- [ ] No swallowed exceptions
- [ ] No `catch (Exception) {}` without logging or mapping

### Evidence Required

- Error taxonomy
- Tests for failure paths
- Example error surfaces (UI or logs)

### Current State

**Assessed: 2026-02-02**

✅ **Exception hierarchy exists:**
- `VoltException` — base with ErrorCode
- `ConnectionException` — network failures (tracks Endpoint, Backend)
- `InferenceException` — LLM failures (tracks Model, Backend)
- `ModelNotFoundException` — missing model errors

✅ **Health check model exists:**
- `HealthCheckResult` with `IsHealthy`, `Status`, `ResponseTime`
- GPU metrics: `GpuMemoryUsed`, `GpuMemoryTotal`
- Factory methods: `Healthy()`, `Unhealthy()`

✅ **ViewModel error handling pattern:**
- `ViewModelBase.SetError()` logs and displays
- `ExecuteAsync()` catches and maps exceptions
- `OperationCanceledException` handled separately

⛔ **No service implementations to verify:**
- Cannot audit exception handling without implementations
- Cannot verify error messages are user-safe

### Blocking Issues

1. Implement all services with proper exception handling
2. Add tests for at least 5 failure scenarios:
   - Connection timeout
   - Model not found
   - Malformed response
   - GPU memory exhaustion
   - Context window exceeded
3. Audit all error messages for user-safety (no stack traces, no internal paths)

### Why This Gate Exists

Silent failure is worse than loud failure.

---

## Gate 6 — Architecture Lock (HUMAN-VERIFIED)

**Status:** ⛔ BLOCKED

**Goal:** Prevent Phase 3 scope creep disguised as refactors.

### Acceptance Criteria

- [ ] `ARCHITECTURE.md` updated to Phase 2 state
- [ ] Explicit out-of-scope list
- [ ] Explicit extension points
- [ ] Explicit non-goals

### Evidence Required

- Doc diff review
- Sign-off before Phase 3 starts

### Current State

**Assessed: 2026-02-02**

Current `ARCHITECTURE.md` documents Phase 1 structure accurately.

### Required Updates for Phase 2

Add the following sections to `ARCHITECTURE.md`:

#### Out-of-Scope for Phase 2
- Multi-user support
- Cloud sync
- Plugin system
- Custom model training
- Voice input/output

#### Extension Points
- `IInferenceClient` — add new LLM backends
- `IConversationStorage` — add new storage backends
- Configuration options — add new settings sections

#### Non-Goals
- Cross-platform support (Windows-only)
- Web interface
- Mobile companion app
- Model fine-tuning

### Blocking Issues

1. Update `ARCHITECTURE.md` with Phase 2 sections
2. Human sign-off required before Phase 3

### Why This Gate Exists

Most projects die here. This gate prevents that.

---

## Gate 7 — User-Visible Trust Signals (ENFORCED)

**Status:** ⛔ BLOCKED

**Goal:** Users can tell Volt is behaving correctly.

### Acceptance Criteria

- [ ] Clear loading states
- [ ] Clear inference-in-progress signal
- [ ] Clear failure messaging
- [ ] No ambiguous "nothing happened" states

### Evidence Required

- Screenshots or recordings
- ViewModel tests validating state transitions

### Current State

**Assessed: 2026-02-02**

✅ **ViewModel properties exist:**
- `ViewModelBase.IsLoading`, `IsBusy`, `ErrorMessage`, `HasError`
- `ChatViewModel.IsGenerating`, `StatusText`, `TokensPerSecond`
- `MessageViewModel.IsStreaming`
- `SettingsViewModel.IsConnected`, `ConnectionStatus`

✅ **State transitions implemented in code:**
```csharp
// ChatViewModel.SendAsync()
IsGenerating = true;
// ... streaming ...
IsGenerating = false;
```

⛔ **No UI to verify:**
- Views not implemented (no XAML bindings)
- Cannot take screenshots
- Cannot verify user experience

⛔ **No state transition tests:**
- No tests for Idle → Loading → Success
- No tests for Idle → Loading → Error
- No tests for ambiguous state prevention

### Blocking Issues

1. Implement UI Views with proper bindings
2. Add ViewModel state transition tests
3. Capture screenshots/recordings of:
   - Idle state
   - Loading/generating state
   - Success state
   - Error state
4. Document all valid state combinations

### Why This Gate Exists

Trust is built in moments of waiting and failure.

---

## Phase 2 Completion Definition

Phase 2 is complete when:

1. All **ENFORCED** gates pass automatically
2. All **HUMAN-VERIFIED** gates are signed off
3. No new features were added unless required to satisfy a gate

At that point, Volt becomes:

> **Operationally credible**
>
> Not polished.
> Not scaled.
> But safe, explainable, and trustworthy.

---

## Phase 2 Non-Goals

- Multi-user support
- Cloud sync or backup
- Plugin/extension system
- Custom model training or fine-tuning
- Voice input/output
- Cross-platform support
- Web interface
- Mobile companion app
- Ollama installation/management (user responsibility)

---

## Phase 2 Work Packages

### WP1: Service Implementations (Gates 3, 5)
- [ ] `ChatService` with conversation orchestration
- [ ] `SettingsService` with JSON persistence
- [ ] `JsonConversationStorage` with file I/O
- [ ] `NavigationService` for WinUI 3

### WP2: Inference Layer (Gates 3, 5)
- [ ] `OllamaInferenceClient` with HTTP client
- [ ] `InferenceClientFactory` for backend selection
- [ ] `OllamaModelManager` for model lifecycle

### WP3: Test Coverage (Gate 2)
- [ ] Serialization round-trip tests
- [ ] Error path tests (5+ scenarios)
- [ ] State transition tests
- [ ] Coverage threshold enforcement

### WP4: UI Implementation (Gate 7)
- [ ] ChatView with message rendering
- [ ] SettingsView with connection testing
- [ ] ConversationListView with selection
- [ ] State-to-UI binding verification

### WP5: Documentation (Gate 6)
- [ ] Update ARCHITECTURE.md
- [ ] Document out-of-scope items
- [ ] Document extension points
- [ ] Human sign-off

---

## Changelog

| Date | Gate | Change | Evidence |
|------|------|--------|----------|
| 2026-02-02 | All | Initial gate document created | — |
| 2026-02-02 | All | Complete gate assessment performed | Build/test transcripts, coverage reports, code review |
