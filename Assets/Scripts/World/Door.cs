using System;
using UnityEngine;

/// <summary>
/// Kapı: kapalıyken oyuncuyu fiziksel olarak engeller, açılınca collider kapanır.
/// Hem şalter (Lever) hem de ileride NPC (door_open: true) tarafından açılabilir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float openAlpha = 0.15f;

    private Collider2D _collider;
    private SpriteRenderer _renderer;

    public bool IsOpen { get; private set; }
    public event Action Opened;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        _collider.enabled = false;

        Color c = _renderer.color;
        c.a = openAlpha;
        _renderer.color = c;

        Debug.Log($"[Door] {name} açıldı.");
        Opened?.Invoke();
    }
}
