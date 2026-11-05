using UnityEngine;

/// <summary>
/// Ortografik kamerayı, ekran oranı ne olursa olsun (16:9, 16:10, 4:3...) haritanın
/// tamamını gösterecek şekilde ayarlar. Pencere boyutu değişirse yeniden hesaplar.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [Tooltip("Haritanın dünya birimi cinsinden boyutu (genişlik, yükseklik).")]
    [SerializeField] private Vector2 levelSize = new Vector2(32f, 16f);
    [Tooltip("Haritanın merkezi.")]
    [SerializeField] private Vector2 levelCenter = new Vector2(16f, 8f);
    [SerializeField] private float padding = 0.3f;

    private Camera _camera;
    private float _lastAspect;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        transform.position = new Vector3(levelCenter.x, levelCenter.y, transform.position.z);
        Fit();
    }

    private void Update()
    {
        if (!Mathf.Approximately(_camera.aspect, _lastAspect)) Fit();
    }

    private void Fit()
    {
        _lastAspect = _camera.aspect;
        float sizeForHeight = levelSize.y / 2f;
        float sizeForWidth = levelSize.x / 2f / _camera.aspect;
        _camera.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth) + padding;
    }
}
