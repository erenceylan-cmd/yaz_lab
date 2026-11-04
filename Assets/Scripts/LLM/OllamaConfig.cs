using System;
using UnityEngine;

/// <summary>Ollama bağlantı ayarları (Inspector'dan değiştirilebilir).</summary>
[Serializable]
public class OllamaConfig
{
    public string baseUrl = "http://localhost:11434";
    public string model = "llama3.2:3b";

    [Tooltip("Tek bir yanıt için en fazla bekleme süresi (sn). İlk istekte model belleğe yüklendiği için daha uzun sürebilir.")]
    public float timeoutSeconds = 60f;

    [Range(0f, 1.5f)] public float temperature = 0.7f;

    [Tooltip("Yanıttaki en fazla token sayısı.")]
    public int maxTokens = 256;
}
