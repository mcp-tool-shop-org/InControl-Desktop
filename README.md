# Volt

**Local AI Chat Assistant for Windows**

A privacy-first, GPU-accelerated chat application that runs large language models entirely on your machine. No cloud required.

## Why Volt?

- **Private by default** - Your conversations never leave your computer
- **RTX-optimized** - Built for NVIDIA GPUs with CUDA acceleration
- **Native Windows experience** - WinUI 3 with Fluent Design
- **Multiple backends** - Ollama, llama.cpp, or bring your own
- **Markdown rendering** - Rich text, code blocks, and syntax highlighting

## Target Hardware

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| GPU | RTX 3060 (8GB) | RTX 4080/5080 (16GB) |
| RAM | 16GB | 32GB |
| OS | Windows 10 1809+ | Windows 11 |
| .NET | 8.0 | 9.0 |

## Quick Start

```bash
# Clone and build
git clone https://github.com/mcp-tool-shop-org/Volt.git
cd Volt
dotnet build

# Run (requires Ollama running locally)
dotnet run --project src/Volt.App
```

## Architecture

Volt follows a clean, layered architecture:

```
┌─────────────────────────────────────────┐
│            Volt.App (WinUI 3)           │  UI Layer
├─────────────────────────────────────────┤
│           Volt.ViewModels               │  Presentation
├─────────────────────────────────────────┤
│           Volt.Services                 │  Business Logic
├─────────────────────────────────────────┤
│           Volt.Inference                │  LLM Backends
├─────────────────────────────────────────┤
│           Volt.Core                     │  Shared Types
└─────────────────────────────────────────┘
```

See [ARCHITECTURE.md](./docs/ARCHITECTURE.md) for detailed design documentation.

## Development Phases

### Phase 1: Foundation (Current)
Deterministic infrastructure layers - project structure, dependency injection, configuration, logging, and core abstractions.

### Phase 2: Inference
LLM backend integration - Ollama client, streaming responses, model management.

### Phase 3: UI
WinUI 3 chat interface - message bubbles, markdown rendering, settings.

### Phase 4: Polish
Performance optimization, error handling, accessibility, packaging.

## Tech Stack

| Layer | Technology |
|-------|------------|
| UI Framework | WinUI 3 (Windows App SDK 1.6) |
| Architecture | MVVM with CommunityToolkit.Mvvm |
| LLM Integration | OllamaSharp, Microsoft.Extensions.AI |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Configuration | Microsoft.Extensions.Configuration |
| Logging | Microsoft.Extensions.Logging + Serilog |

## License

MIT

## Contributing

Contributions welcome! Please read the contributing guidelines before submitting PRs.

---

*Built for Windows. Powered by local AI.*
