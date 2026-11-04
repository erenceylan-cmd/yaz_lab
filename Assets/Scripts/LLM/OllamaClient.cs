using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Ollama /api/chat uç noktasıyla konuşan ILLMClient uygulaması.
/// HttpClient + async/await kullanır: ağ beklemesi ana thread'i (frame rate'i) bloklamaz.
/// Tüm ağ/format hataları LLMException'a çevrilir.
/// </summary>
public class OllamaClient : ILLMClient, IDisposable
{
    private readonly OllamaConfig _config;
    private readonly HttpClient _http;

    public OllamaClient(OllamaConfig config)
    {
        _config = config;
        // Zaman aşımını kendimiz (CancellationToken ile) yönetiyoruz.
        _http = new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
    }

    public async Task<string> CompleteAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        string body = BuildRequestJson(request);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_config.timeoutSeconds));

        HttpResponseMessage response;
        string responseText;
        try
        {
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            response = await _http.PostAsync(Url("/api/chat"), content, timeoutCts.Token);
            responseText = await response.Content.ReadAsStringAsync();
        }
        catch (OperationCanceledException e)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new LLMException(LLMErrorKind.Cancelled, "İstek iptal edildi.", e);
            throw new LLMException(LLMErrorKind.Timeout, $"{_config.timeoutSeconds} sn içinde yanıt gelmedi.", e);
        }
        catch (HttpRequestException e)
        {
            throw new LLMException(LLMErrorKind.ServerUnreachable, $"Sunucuya bağlanılamadı: {e.Message}", e);
        }

        if (!response.IsSuccessStatusCode)
        {
            string error = TryReadError(responseText);
            bool modelMissing = (int)response.StatusCode == 404 || (error?.Contains("not found") ?? false);
            throw new LLMException(
                modelMissing ? LLMErrorKind.ModelNotFound : LLMErrorKind.BadResponse,
                $"HTTP {(int)response.StatusCode}: {error ?? responseText}");
        }

        OllamaChatResponse parsed;
        try
        {
            parsed = JsonUtility.FromJson<OllamaChatResponse>(responseText);
        }
        catch (Exception e)
        {
            throw new LLMException(LLMErrorKind.BadResponse, "Sunucu yanıtı çözümlenemedi.", e);
        }

        if (parsed?.message == null || string.IsNullOrWhiteSpace(parsed.message.content))
            throw new LLMException(LLMErrorKind.BadResponse, "Model boş yanıt döndürdü.");

        return parsed.message.content;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            HttpResponseMessage response = await _http.GetAsync(Url("/api/tags"), cts.Token);
            if (!response.IsSuccessStatusCode) return false;

            string json = await response.Content.ReadAsStringAsync();
            OllamaTagsResponse tags = JsonUtility.FromJson<OllamaTagsResponse>(json);
            if (tags?.models == null) return false;

            foreach (OllamaModelInfo m in tags.models)
                if (m.name == _config.model || m.name == _config.model + ":latest")
                    return true;

            Debug.LogWarning($"[Ollama] Sunucu açık ama '{_config.model}' modeli yüklü değil.");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Ollama] Sunucu kontrolü başarısız: {e.Message}");
            return false;
        }
    }

    public void Dispose() => _http.Dispose();

    // ---------------- yardımcılar ----------------

    private string Url(string path) => _config.baseUrl.TrimEnd('/') + path;

    private string BuildRequestJson(LLMRequest request)
    {
        var dto = new OllamaChatRequest
        {
            model = _config.model,
            stream = false,
            keep_alive = "15m",
            messages = new ChatMessage[request.Messages.Count],
            options = new OllamaOptions
            {
                temperature = _config.temperature,
                num_predict = _config.maxTokens
            }
        };
        for (int i = 0; i < request.Messages.Count; i++) dto.messages[i] = request.Messages[i];

        string json = JsonUtility.ToJson(dto);

        // JsonUtility iç içe serbest JSON (şema) yazamaz; "format" alanını elle ekliyoruz.
        string format = string.IsNullOrEmpty(request.JsonSchema) ? null : request.JsonSchema;
        if (format != null)
            json = json.Substring(0, json.Length - 1) + ",\"format\":" + format + "}";

        return json;
    }

    private static string TryReadError(string body)
    {
        try { return JsonUtility.FromJson<OllamaError>(body)?.error; }
        catch { return null; }
    }

    // ---------------- Ollama JSON veri sınıfları ----------------

    [Serializable] private class OllamaChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public bool stream;
        public string keep_alive;
        public OllamaOptions options;
    }

    [Serializable] private class OllamaOptions
    {
        public float temperature;
        public int num_predict;
    }

    [Serializable] private class OllamaChatResponse
    {
        public ChatMessage message;
        public bool done;
    }

    [Serializable] private class OllamaTagsResponse { public OllamaModelInfo[] models; }
    [Serializable] private class OllamaModelInfo { public string name; }
    [Serializable] private class OllamaError { public string error; }
}
