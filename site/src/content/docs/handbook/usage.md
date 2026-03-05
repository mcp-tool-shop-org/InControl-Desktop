---
title: Usage
description: Building, testing, and working with InControl Desktop day-to-day.
sidebar:
  order: 2
---

## Verify your build environment

Run the verification script to check that all dependencies are in place:

```powershell
./scripts/verify.ps1
```

## Development build

```bash
dotnet build
```

## Release build

The release script creates artifacts in the `artifacts/` directory:

```powershell
./scripts/release.ps1
```

## Run tests

```bash
dotnet test
```

## Multiple backends

InControl supports several LLM backends:

- **Ollama** — the default. Install from [ollama.ai](https://ollama.ai/), pull a model, and run `ollama serve`.
- **llama.cpp** — direct inference via llama.cpp bindings.
- **Bring your own** — implement the inference interface to connect any backend.

Switch backends without changing your workflow. The `InControl.Inference` package provides a unified abstraction layer with streaming chat, model management, and health checks.

## NuGet packages in your own projects

The core libraries are standalone NuGet packages you can use independently:

```bash
dotnet add package InControl.Core
dotnet add package InControl.Inference
```

Example — stream chat responses from Ollama:

```csharp
var client = inferenceClientFactory.Create("ollama");
await foreach (var token in client.StreamChatAsync(messages))
{
    Console.Write(token);
}
```
