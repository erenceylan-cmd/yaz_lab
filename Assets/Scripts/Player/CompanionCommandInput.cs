using UnityEngine;

/// <summary>
/// Oyuncunun companion'a komut vermesi:
///   E: en yakın etkileşilebilir nesneye (şalter) git ve kullan
///   F: beni takip et
/// Companion'ın kendisi hedefi bilmez; sadece IInteractable alır.
/// </summary>
public class CompanionCommandInput : MonoBehaviour
{
    [SerializeField] private CompanionAgent companion;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode followKey = KeyCode.F;

    private void Update()
    {
        if (companion == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            IInteractable target = FindNearestInteractable();
            if (target == null)
                Debug.Log("[Komut] Etkileşilecek bir şey kalmadı.");
            else if (!companion.TryCommandInteract(target))
                Debug.LogWarning("[Komut] Companion hedefe ulaşamıyor.");
        }

        if (Input.GetKeyDown(followKey))
            companion.CommandFollow();
    }

    private IInteractable FindNearestInteractable()
    {
        IInteractable nearest = null;
        float best = float.PositiveInfinity;

        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is not IInteractable interactable || !interactable.CanInteract) continue;

            float d = Vector2.Distance(companion.transform.position, interactable.InteractionPoint.position);
            if (d < best)
            {
                best = d;
                nearest = interactable;
            }
        }
        return nearest;
    }
}
