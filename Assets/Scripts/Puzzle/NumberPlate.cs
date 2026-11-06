using System;
using UnityEngine;

/// <summary>
/// Numaralı basınç plakası. Oyuncunun MERKEZİ plakanın üstüne geldiğinde bir kez "basılır".
/// (Merkez kontrolü sayesinde plakaların arasından yürürken yanlışlıkla basılmaz.)
/// Companion'ın collider'ı olmadığı için companion plakaya basmaz, sadece üstünde durup işaret eder.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class NumberPlate : MonoBehaviour
{
    [SerializeField, Range(1, 9)] private int number = 1;
    [SerializeField] private Color idleColor = new Color(0.55f, 0.62f, 0.8f);

    public int Number => number;
    public event Action<NumberPlate> Pressed;

    private SpriteRenderer _renderer;
    private Transform _player;
    private bool _occupied;
    private GUIStyle _style;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = idleColor;
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) _player = player.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        Bounds b = _renderer.bounds;
        bool inside = b.Contains(new Vector3(_player.position.x, _player.position.y, b.center.z));

        if (inside && !_occupied)
        {
            _occupied = true;
            Pressed?.Invoke(this);
        }
        else if (!inside)
        {
            _occupied = false;
        }
    }

    public void SetColor(Color color) => _renderer.color = color;
    public void ResetColor() => _renderer.color = idleColor;

    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        _style ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.1f, 0.1f, 0.15f) }
        };

        Vector3 p = cam.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(p.x - 20, Screen.height - p.y - 15, 40, 30), number.ToString(), _style);
    }
}
