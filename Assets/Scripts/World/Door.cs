using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kapı: kapalıyken oyuncuyu fiziksel olarak engeller (collider), açılınca geçilebilir olur.
/// NavMesh tarafında kapının kendi alanı (Navigation Area) vardır: kapı kapalıyken bu alan
/// ajanların areaMask'inden çıkarılır, açılınca eklenir. Böylece companion kapalı kapıdan
/// yol çizmez, kapı açılınca yolu otomatik olarak yeniden hesaplar.
/// Şalter, basınç plakası veya NPC (door_open: true) tarafından açılabilir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float openAlpha = 0.15f;

    [Tooltip("Bu kapının Navigation Modifier'ında seçili alan adı (örn. TunnelGate). Boşsa NavMesh'e dokunulmaz.")]
    [SerializeField] private string navMeshArea = "";

    private Collider2D _collider;
    private SpriteRenderer _renderer;
    private Color _closedColor;
    private int _areaBit;

    public bool IsOpen { get; private set; }
    public event Action Opened;
    public event Action Closed;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _closedColor = _renderer.color;

        if (!string.IsNullOrEmpty(navMeshArea))
        {
            int area = NavMesh.GetAreaFromName(navMeshArea);
            if (area < 0)
                Debug.LogWarning($"[Door] {name}: '{navMeshArea}' adında bir NavMesh alanı yok (Navigation → Areas).");
            else
                _areaBit = 1 << area;
        }
    }

    private void Start() => ApplyNavMeshState();

    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        _collider.enabled = false;

        Color c = _closedColor;
        c.a = openAlpha;
        _renderer.color = c;

        ApplyNavMeshState();
        Debug.Log($"[Door] {name} açıldı.");
        Opened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        _collider.enabled = true;
        _renderer.color = _closedColor;

        ApplyNavMeshState();
        Debug.Log($"[Door] {name} kapandı.");
        Closed?.Invoke();
    }

    /// <summary>Kapının durumunu sahnedeki tüm NavMesh ajanlarının alan maskesine yansıtır.</summary>
    private void ApplyNavMeshState()
    {
        if (_areaBit == 0) return;

        foreach (NavMeshAgent agent in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
        {
            agent.areaMask = IsOpen ? agent.areaMask | _areaBit : agent.areaMask & ~_areaBit;

            // Mevcut yolu yeni duruma göre yeniden hesaplat (kapı kapandıysa yol "partial" olur).
            if (agent.isOnNavMesh && agent.hasPath)
                agent.SetDestination(agent.destination);
        }
    }
}
