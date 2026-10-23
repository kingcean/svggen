# SVG Icon Generator by AI

## Project

This is the main project for the SVG Icon Generator by AI.
It contains the core logic and functionality of the application.
It also contains a CLI tool that allows users to generate SVG icons using AI models.

## Settings

```json
{
  "chat": {
    "apiKey": "your-api-key",
    "apiEndpoint": "https://api.openai.com/v1",
    "chatModel": "your-chat-model"
  },
  "workspace": {
    "dir": "path/to/your/workspace"
  }
}
```

| Chat property | Environment variable | JSON path |
| --- | --- | --- |
| `ApiKey` | `OPENAI_API_KEY` | `chat.apiKey` |
| `ApiEndpoint` | `OPENAI_API_ENDPOINT` | `chat.apiEndpoint` |
| `ChatModel` | `OPENAI_API_CHAT_MODEL` | `chat.chatModel` |
