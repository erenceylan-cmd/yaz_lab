using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Dil modeli istemcisi soyutlaması. Oyun kodu sadece bunu tanır;
/// Ollama yerine başka bir yerel sunucu (LM Studio vb.) takılabilir.
/// Hata durumunda LLMException fırlatır.
/// </summary>
public interface ILLMClient
{
    /// <summary>Modele istek gönderir ve modelin ham metin çıktısını döndürür.</summary>
    Task<string> CompleteAsync(LLMRequest request, CancellationToken cancellationToken);

    /// <summary>Sunucu açık ve model yüklü mü? Hata fırlatmaz.</summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}
