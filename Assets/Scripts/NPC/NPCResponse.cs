using System;

/// <summary>
/// NPC'nin modelden beklenen yapılandırılmış yanıtı.
/// Alan adları JSON şemasıyla birebir aynıdır (JsonUtility bu isimlerle eşleştirir).
/// </summary>
[Serializable]
public class NPCResponse
{
    public string reply;      // NPC'nin oyuncuya söylediği söz
    public bool door_open;    // true => kapı açılır (oyun durumu değişir)
    public string mood;       // neutral | friendly | suspicious | angry

    public static readonly string[] AllowedMoods = { "neutral", "friendly", "suspicious", "angry" };

    /// <summary>Ollama'ya "format" olarak gönderilen JSON şeması.</summary>
    public const string JsonSchema =
        "{\"type\":\"object\",\"properties\":{" +
        "\"reply\":{\"type\":\"string\"}," +
        "\"door_open\":{\"type\":\"boolean\"}," +
        "\"mood\":{\"type\":\"string\",\"enum\":[\"neutral\",\"friendly\",\"suspicious\",\"angry\"]}" +
        "},\"required\":[\"reply\",\"door_open\",\"mood\"]}";
}
