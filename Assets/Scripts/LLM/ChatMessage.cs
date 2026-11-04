using System;

/// <summary>Sohbet geçmişindeki tek bir mesaj. role: "system" | "user" | "assistant"</summary>
[Serializable]
public class ChatMessage
{
    public string role;
    public string content;

    public ChatMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }

    public static ChatMessage System(string text) => new ChatMessage("system", text);
    public static ChatMessage User(string text) => new ChatMessage("user", text);
    public static ChatMessage Assistant(string text) => new ChatMessage("assistant", text);
}
