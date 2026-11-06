using System;
using UnityEngine;

/// <summary>
/// Companion'ın oyuncuya "işaret edebileceği" bir ipucu kaynağı (örn. şifreli plaka bulmacası).
/// Companion bulmacanın kendisini değil, sadece bu arayüzü tanır.
/// </summary>
public interface IHintProvider
{
    /// <summary>Şu an gösterilecek bir ipucu var mı? (bulmaca çözüldüyse false)</summary>
    bool HasHint { get; }

    /// <summary>İşaret edilecek konum (örn. sıradaki doğru plakanın merkezi).</summary>
    Vector3 HintPosition { get; }

    /// <summary>İpucu değiştiğinde (ilerleme, hata, çözülme) tetiklenir.</summary>
    event Action HintChanged;
}
