using System;

/// <summary>
/// NPC'nin modelden beklenen yapılandırılmış yanıtı.
/// Alan adları JSON şemasıyla birebir aynıdır (JsonUtility bu isimlerle eşleştirir).
/// </summary>
[Serializable]
public class NPCResponse
{
    // Alan sırası bilinçlidir: model önce sözünü ve ruh halini üretir, kapı kararını EN SON verir.
    // Böylece karar, az önce yazdığı cevapla tutarlı olur.
    public string reply;      // NPC'nin oyuncuya söylediği söz
    public string mood;       // neutral | friendly | suspicious | angry
    public bool door_open;    // true => kapı açılır (oyun durumu değişir)

    public static readonly string[] AllowedMoods = { "neutral", "friendly", "suspicious", "angry" };

    /// <summary>Ollama'ya "format" olarak gönderilen JSON şeması.</summary>
    public const string JsonSchema =
        "{\"type\":\"object\",\"properties\":{" +
        "\"reply\":{\"type\":\"string\"}," +
        "\"mood\":{\"type\":\"string\",\"enum\":[\"neutral\",\"friendly\",\"suspicious\",\"angry\"]}," +
        "\"door_open\":{\"type\":\"boolean\"}" +
        "},\"required\":[\"reply\",\"mood\",\"door_open\"]}";
}
