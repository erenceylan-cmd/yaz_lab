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
    private bool _activated;

    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;
    public bool CanInteract => !_activated;

    private void Awake() => _renderer = GetComponent<SpriteRenderer>();

    public void Interact(GameObject interactor)
    {
        if (_activated) return;

        _activated = true;
        _renderer.color = activatedColor;
        Debug.Log($"[Lever] {name}, {interactor.name} tarafından indirildi.");
        onActivated?.Invoke();
    }
}
