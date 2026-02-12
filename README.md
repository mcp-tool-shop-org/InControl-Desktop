# InControl-Desktop

**Local AI Chat Assistant for Windows**

A privacy-first, GPU-accelerated chat application that runs large language models entirely on your machine. No cloud required.

## Why InControl?

- **Private by default** - Your conversations never leave your computer
- **RTX-optimized** - Built for NVIDIA GPUs with CUDA acceleration
- **Native Windows experience** - WinUI 3 with Fluent Design
- **Multiple backends** - Ollama, llama.cpp, or bring your own
- **Markdown rendering** - Rich text, code blocks, and syntax highlighting

## NuGet Packages

The core libraries are available as standalone NuGet packages for building your own local AI integrations:

| Package | Description |
|---------|-------------|
| `InControl.Core` | Domain models, conversation types, and shared abstractions for local AI chat applications. |
| `InControl.Inference` | LLM backend abstraction layer with streaming chat, model management, and health checks. Includes Ollama implementation. |

```csharp
// Example: use InControl.Inference in your own app
var client = inferenceClientFactory.Create("ollama");
await foreach (var token in client.StreamChatAsync(messages))
{
    Console.Write(token);
}
```

## Target Hardware

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| GPU | RTX 3060 (8GB) | RTX 4080/5080 (16GB) |
| RAM | 16GB | 32GB |
| OS | Windows 10 1809+ | Windows 11 |
| .NET | 9.0 | 9.0 |

## Installation

### From Release (Recommended)

1. Download the latest MSIX package from [Releases](https://github.com/mcp-tool-shop-org/InControl-Desktop/releases)
2. Double-click to install
3. Launch from Start Menu

### From Source

```bash
# Clone and build
git clone https://github.com/mcp-tool-shop-org/InControl-Desktop.git
cd InControl-Desktop
dotnet restore
dotnet build

# Run (requires Ollama running locally)
dotnet run --project src/InControl.App
```

## Prerequisites

InControl requires a local LLM backend. We recommend [Ollama](https://ollama.ai/):

```bash
# Install Ollama from https://ollama.ai/download

# Pull a model
ollama pull llama3.2

# Start the server (runs on http://localhost:11434)
ollama serve
```

## Building

### Verify Build Environment

```powershell
# Run verification script
./scripts/verify.ps1
```

### Development Build

```bash
dotnet build
```

### Release Build

```powershell
# Creates release artifacts in artifacts/
./scripts/release.ps1
```

### Run Tests

```bash
dotnet test
```

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

See [ARCHITECTURE.md](./docs/ARCHITECTURE.md) for detailed design documentation.

## Data Storage

All data is stored locally:

| Data | Location |
|------|----------|
| Sessions | `%LOCALAPPDATA%\InControl\sessions\` |
| Logs | `%LOCALAPPDATA%\InControl\logs\` |
| Cache | `%LOCALAPPDATA%\InControl\cache\` |
| Exports | `%USERPROFILE%\Documents\InControl\exports\` |

See [PRIVACY.md](./docs/PRIVACY.md) for complete data handling documentation.

## Troubleshooting

Common issues and solutions are documented in [TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md).

### Quick Fixes

**App won't start:**
- Check that .NET 9.0 Runtime is installed
- Run `dotnet --list-runtimes` to verify

**No models available:**
- Ensure Ollama is running: `ollama serve`
- Pull a model: `ollama pull llama3.2`

**GPU not detected:**
- Update NVIDIA drivers to latest version
- Check CUDA toolkit installation

## Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Submit a pull request

## Reporting Issues

1. Check [TROUBLESHOOTING.md](./docs/TROUBLESHOOTING.md) first
2. Use the "Copy Diagnostics" feature in the app
3. Open an issue with diagnostics info attached

## Tech Stack

| Layer | Technology |
|-------|------------|
| UI Framework | WinUI 3 (Windows App SDK 1.6) |
| Architecture | MVVM with CommunityToolkit.Mvvm |
| LLM Integration | OllamaSharp, Microsoft.Extensions.AI |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Configuration | Microsoft.Extensions.Configuration |
| Logging | Microsoft.Extensions.Logging + Serilog |

## Version

Current version: **0.4.0-alpha**

See [CHANGELOG.md](./CHANGELOG.md) for release history.

## License

MIT

---

*Built for Windows. Powered by local AI.*
