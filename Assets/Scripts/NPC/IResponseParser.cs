/// <summary>Modelin ham metin çıktısını tipli bir nesneye çeviren ayrıştırıcı sözleşmesi.</summary>
public interface IResponseParser<T>
{
    /// <summary>Başarılıysa true ve result döner; değilse false ve error döner. Asla istisna fırlatmaz.</summary>
    bool TryParse(string raw, out T result, out string error);
}
