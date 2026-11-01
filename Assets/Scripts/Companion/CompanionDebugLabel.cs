using UnityEngine;

/// <summary>
/// Companion'ın mevcut durumunu başının üstünde yazar (demo ve ekran görüntüleri için).
/// </summary>
[RequireComponent(typeof(CompanionAgent))]
public class CompanionDebugLabel : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);

    private CompanionAgent _companion;
    private Camera _camera;
    private GUIStyle _style;

    private void Awake()
    {
        _companion = GetComponent<CompanionAgent>();
        _camera = Camera.main;
    }

    private void OnGUI()
    {
        var state = _companion.StateMachine?.Current;
        if (state == null || _camera == null) return;

        _style ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            normal = { textColor = Color.white }
        };

        Vector3 screen = _camera.WorldToScreenPoint(transform.position + offset);
        GUI.Label(new Rect(screen.x - 60f, Screen.height - screen.y - 12f, 120f, 24f), state.Name, _style);
    }
}
