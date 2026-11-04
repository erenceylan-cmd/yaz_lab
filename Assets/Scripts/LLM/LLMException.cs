using System;

public enum LLMErrorKind
{
    ServerUnreachable, // Ollama kapalı / bağlantı yok
    Timeout,           // Belirlenen sürede yanıt gelmedi
    ModelNotFound,     // Model indirilmemiş (ollama pull yapılmamış)
    BadResponse,       // Sunucu hatası veya beklenmeyen yanıt formatı
    Cancelled          // İstek iptal edildi (sahne kapandı vb.)
}

/// <summary>LLM katmanındaki tüm hatalar tek tip olarak bu istisnayla bildirilir.</summary>
public class LLMException : Exception
{
    public LLMErrorKind Kind { get; }

    public LLMException(LLMErrorKind kind, string message, Exception inner = null)
        : base(message, inner)
    {
        Kind = kind;
    }

    /// <summary>Oyuncuya gösterilecek anlaşılır mesaj.</summary>
    public string UserMessage => Kind switch
    {
        LLMErrorKind.ServerUnreachable => "LLM sunucusuna ulaşılamıyor. Ollama çalışıyor mu? (ollama serve)",
        LLMErrorKind.Timeout => "Model zamanında yanıt vermedi. Tekrar deneyin.",
        LLMErrorKind.ModelNotFound => "Model yüklü değil. Terminalde 'ollama pull <model>' çalıştırın.",
        LLMErrorKind.Cancelled => "İstek iptal edildi.",
        _ => "Modelden geçersiz bir yanıt geldi. Tekrar deneyin."
    };
}
