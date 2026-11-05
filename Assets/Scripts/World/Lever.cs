using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Şalter: companion etkileşime girince bir kez tetiklenir ve bağlı olayları çalıştırır
/// (örn. Door.Open). Bağlantı Inspector'daki On Activated listesinden yapılır.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private Color activatedColor = Color.green;
    [SerializeField] private UnityEvent onActivated;

    private SpriteRenderer _renderer;
    private Color _initialColor;
    private bool _activated;

    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;
    public bool CanInteract => !_activated;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _initialColor = _renderer.color;
    }

    public void Interact(GameObject interactor)
    {
        if (_activated) return;

        _activated = true;
        _renderer.color = activatedColor;
        Debug.Log($"[Lever] {name}, {interactor.name} tarafından indirildi.");
        onActivated?.Invoke();
    }

    /// <summary>Test için şalteri başlangıç durumuna döndürür.</summary>
    public void ResetLever()
    {
        _activated = false;
        _renderer.color = _initialColor;
    }
}
