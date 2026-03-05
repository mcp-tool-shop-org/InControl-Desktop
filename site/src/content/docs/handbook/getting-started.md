---
title: Getting Started
description: Install InControl Desktop and chat with your first local model.
sidebar:
  order: 1
---

InControl Desktop is a privacy-first chat application that runs large language models entirely on your Windows machine. This guide walks you through installation and your first conversation.

## Prerequisites

InControl requires a local LLM backend. We recommend [Ollama](https://ollama.ai/):

```bash
# Install Ollama from https://ollama.ai/download

# Pull a model
ollama pull llama3.2

# Start the server (runs on http://localhost:11434)
ollama serve
```

## Target hardware

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| GPU | RTX 3060 (8GB) | RTX 4080/5080 (16GB) |
| RAM | 16GB | 32GB |
| OS | Windows 10 1809+ | Windows 11 |
| .NET | 9.0 | 9.0 |

## Install from release (recommended)

1. Download the latest MSIX package from [Releases](https://github.com/mcp-tool-shop-org/InControl-Desktop/releases)
2. Double-click to install
3. Launch from the Start Menu

## Install from source

```bash
git clone https://github.com/mcp-tool-shop-org/InControl-Desktop.git
cd InControl-Desktop
dotnet restore
dotnet build

# Run (requires Ollama running locally)
dotnet run --project src/InControl.App
```

## First conversation

1. Make sure Ollama is running (`ollama serve`)
2. Launch InControl Desktop
3. Select a model from the dropdown
4. Type your message and press Enter
5. Responses stream in real-time with markdown rendering, code blocks, and syntax highlighting

All conversations are stored locally on your machine — nothing ever leaves your computer.
