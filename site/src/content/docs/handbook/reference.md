---
title: Reference
description: NuGet packages, hardware requirements, and security scope.
sidebar:
  order: 4
---

## NuGet packages

| Package | Description |
|---------|-------------|
| InControl.Core | Domain models, conversation types, and shared abstractions for local AI chat applications |
| InControl.Inference | LLM backend abstraction layer with streaming chat, model management, and health checks. Includes Ollama implementation |

```bash
dotnet add package InControl.Core
dotnet add package InControl.Inference
```

## Usage example

```csharp
var client = inferenceClientFactory.Create("ollama");
await foreach (var token in client.StreamChatAsync(messages))
{
    Console.Write(token);
}
```

## Security and data scope

| Aspect | Detail |
|--------|--------|
| Data accessed | Local Ollama API (localhost), chat history in local storage, model configuration files |
| Data NOT accessed | No cloud sync, no telemetry, no analytics |
| Permissions | Localhost network (Ollama API), file system for chat history. MSIX sandboxed |

See [SECURITY.md](https://github.com/mcp-tool-shop-org/InControl-Desktop/blob/main/SECURITY.md) for vulnerability reporting.
