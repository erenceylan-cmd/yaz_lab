using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Model çıktısını NPCResponse'a çevirir ve doğrular.
/// Küçük modeller bazen JSON'u ``` blokları veya açıklama metniyle sarar;
/// ilk '{' ile son '}' arası alınarak bu durum tolere edilir.
/// Bozuk/eksik çıktı istisna fırlatmaz, false döner.
/// </summary>
public class NPCResponseParser : IResponseParser<NPCResponse>
{
    public bool TryParse(string raw, out NPCResponse result, out string error)
    {
        result = null;
        error = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Boş çıktı.";
            return false;
        }

        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            error = "Çıktıda JSON nesnesi yok.";
            return false;
        }

        string json = raw.Substring(start, end - start + 1);

        // JsonUtility eksik alanları sessizce varsayılan bırakır; zorunlu alanları elle kontrol ediyoruz.
        if (!json.Contains("\"reply\"") || !json.Contains("\"door_open\""))
        {
            error = "Zorunlu alanlar (reply, door_open) eksik.";
            return false;
        }

        try
        {
            result = JsonUtility.FromJson<NPCResponse>(json);
        }
        catch (Exception e)
        {
            error = "Geçersiz JSON: " + e.Message;
            return false;
        }

        if (result == null || string.IsNullOrWhiteSpace(result.reply))
        {
            error = "reply alanı boş.";
            result = null;
            return false;
        }

        result.reply = result.reply.Trim();
        result.mood = NormalizeMood(result.mood);
        return true;
    }

    private static string NormalizeMood(string mood)
    {
        string m = (mood ?? "").Trim().ToLowerInvariant();
        return NPCResponse.AllowedMoods.Contains(m) ? m : "neutral";
    }
}
