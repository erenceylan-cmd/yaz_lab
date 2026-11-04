using System.Collections.Generic;

/// <summary>
/// Bir LLM isteğinin sağlayıcıdan bağımsız tanımı.
/// JsonSchema verilirse model çıktısı bu şemaya zorlanır (structured output).
/// </summary>
public class LLMRequest
{
    public IReadOnlyList<ChatMessage> Messages { get; }
    public string JsonSchema { get; }

    public LLMRequest(IReadOnlyList<ChatMessage> messages, string jsonSchema = null)
    {
        Messages = messages;
        JsonSchema = jsonSchema;
    }
}
