using System;

/// <summary>
/// Diyalog ekranı soyutlaması. NPC mantığı ekranın nasıl çizildiğini bilmez
/// (IMGUI, Canvas veya başka bir UI ile değiştirilebilir).
/// </summary>
public interface IDialogueView
{
    event Action<string> MessageSubmitted;
    event Action CloseRequested;

    void Open(string npcName);
    void Close();
    void AppendPlayerMessage(string text);
    void AppendNpcMessage(string npcName, string text, string mood);
    void AppendSystemMessage(string text);
    void SetThinking(bool thinking);
    void SetServerStatus(string status);
}
