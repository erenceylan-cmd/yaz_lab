using UnityEngine;

/// <summary>
/// Companion'ın (veya başka bir aktörün) etkileşime girebileceği her nesne bu arayüzü uygular.
/// Örn: Lever (şalter). Companion somut sınıfı değil, bu arayüzü tanır (Dependency Inversion).
/// </summary>
public interface IInteractable
{
    /// <summary>Etkileşim için gidilecek nokta.</summary>
    Transform InteractionPoint { get; }

    /// <summary>Şu an etkileşime açık mı? (örn. şalter zaten indirildiyse false)</summary>
    bool CanInteract { get; }

    void Interact(GameObject interactor);
}
