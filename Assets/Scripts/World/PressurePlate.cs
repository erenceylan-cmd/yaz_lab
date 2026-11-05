using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Basınç plakası: üzerinde fiziksel bir gövde (oyuncu) durduğu sürece basılı kalır.
/// Basılınca On Pressed, bırakılınca On Released olayları çalışır
/// (örn. Door.Open / Door.Close). Bağlantı Inspector'dan yapılır.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Color pressedColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;

    private SpriteRenderer _renderer;
    private Color _idleColor;
    private int _bodiesOnPlate;

    public bool IsPressed => _bodiesOnPlate > 0;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _idleColor = _renderer.color;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return; // sadece fiziksel gövdeler (oyuncu) basabilir

        _bodiesOnPlate++;
        if (_bodiesOnPlate == 1)
        {
            _renderer.color = pressedColor;
            Debug.Log($"[Plate] {name} basıldı.");
            onPressed?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody == null) return;

        _bodiesOnPlate = Mathf.Max(0, _bodiesOnPlate - 1);
        if (_bodiesOnPlate == 0)
        {
            _renderer.color = _idleColor;
            Debug.Log($"[Plate] {name} bırakıldı.");
            onReleased?.Invoke();
        }
    }
}
