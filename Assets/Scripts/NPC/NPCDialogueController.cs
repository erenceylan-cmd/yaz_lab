using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Diyalog mantığı (Unity arayüzünden bağımsız, sade C#):
/// sohbet geçmişini tutar, modele istek atar, çıktıyı ayrıştırır, gerekirse bir kez düzeltme ister.
/// Hiçbir durumda istisna dışarı sızmaz; sonuç her zaman DialogueOutcome olarak döner.
/// </summary>
public class NPCDialogueController
{
    private const int MaxParseRetries = 1;
    private const string RetryInstruction =
        "Önceki çıktın geçerli değildi. SADECE şu şemadaki JSON nesnesini döndür: " +
        "{\"reply\": string, \"mood\": \"neutral\"|\"friendly\"|\"suspicious\"|\"angry\", \"door_open\": boolean}";

    private readonly ILLMClient _client;
    private readonly IResponseParser<NPCResponse> _parser;
    private readonly NPCPersona _persona;
    private readonly int _maxHistoryMessages;
    private readonly List<ChatMessage> _history = new List<ChatMessage>();

    public bool IsBusy { get; private set; }

    public NPCDialogueController(ILLMClient client, IResponseParser<NPCResponse> parser,
                                 NPCPersona persona, int maxHistoryMessages = 12)
    {
        _client = client;
        _parser = parser;
        _persona = persona;
        _maxHistoryMessages = maxHistoryMessages;
    }

    public async Task<DialogueOutcome> SendAsync(string playerText, CancellationToken ct)
    {
        if (IsBusy) return DialogueOutcome.Fail("NPC hâlâ düşünüyor, biraz bekleyin.");
        if (string.IsNullOrWhiteSpace(playerText)) return DialogueOutcome.Fail("Boş mesaj gönderilemez.");

        IsBusy = true;
        var userMessage = ChatMessage.User(playerText.Trim());
        _history.Add(userMessage);

        try
        {
            for (int attempt = 0; attempt <= MaxParseRetries; attempt++)
            {
                List<ChatMessage> messages = BuildMessages(retry: attempt > 0);
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                string raw = await _client.CompleteAsync(new LLMRequest(messages, NPCResponse.JsonSchema), ct);
                Debug.Log($"[NPC] Model yanıtı ({stopwatch.ElapsedMilliseconds} ms): {raw}");

                if (_parser.TryParse(raw, out NPCResponse response, out string parseError))
                {
                    // Geçmişe temiz (normalize edilmiş) JSON'u yazıyoruz ki model formatı korusun.
                    _history.Add(ChatMessage.Assistant(JsonUtility.ToJson(response)));
                    TrimHistory();
                    return DialogueOutcome.Ok(response);
                }

                Debug.LogWarning($"[NPC] Ayrıştırma hatası (deneme {attempt + 1}): {parseError}");
            }

            _history.Remove(userMessage); // oyuncu aynı mesajı tekrar deneyebilsin
            return DialogueOutcome.Fail("Bekçi ne diyeceğini şaşırdı (geçersiz yanıt). Tekrar deneyin.");
        }
        catch (LLMException e)
        {
            Debug.LogWarning($"[NPC] LLM hatası ({e.Kind}): {e.Message}");
            _history.Remove(userMessage);
            return DialogueOutcome.Fail(e.UserMessage);
        }
        catch (System.Exception e)
        {
            // Beklenmeyen her şey: oyun çökmesin.
            Debug.LogException(e);
            _history.Remove(userMessage);
            return DialogueOutcome.Fail("Beklenmeyen bir hata oluştu. Tekrar deneyin.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private List<ChatMessage> BuildMessages(bool retry)
    {
        var list = new List<ChatMessage>(_history.Count + 2) { ChatMessage.System(_persona.systemPrompt) };
        list.AddRange(_history);
        if (retry) list.Add(ChatMessage.System(RetryInstruction));
        return list;
    }

    private void TrimHistory()
    {
        while (_history.Count > _maxHistoryMessages) _history.RemoveAt(0);
    }
}
