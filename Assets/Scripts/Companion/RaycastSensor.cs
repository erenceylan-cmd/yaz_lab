using UnityEngine;

/// <summary>
/// Companion'ın "gözleri": hareket yönünde yelpaze şeklinde Physics2D ışınları atar.
/// - ObstacleAhead: önde engel var mı (takılma tespitinde kullanılır)
/// - HasLineOfSight: iki nokta arasında duvar var mı (etkileşim doğrulamasında kullanılır)
/// Scene görünümünde ışınlar yeşil (boş) / kırmızı (engel) çizilir.
/// </summary>
public class RaycastSensor : MonoBehaviour
{
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float range = 1.2f;
    [SerializeField, Range(1, 9)] private int rayCount = 5;
    [SerializeField, Range(0f, 180f)] private float spreadAngle = 70f;

    public bool ObstacleAhead { get; private set; }
    public float NearestObstacleDistance { get; private set; } = float.PositiveInfinity;

    private Vector2 _forward = Vector2.right;

    /// <summary>Verilen yönde tarama yapar. Yön sıfırsa son yön kullanılır.</summary>
    public void Scan(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.0001f) _forward = direction.normalized;

        ObstacleAhead = false;
        NearestObstacleDistance = float.PositiveInfinity;

        for (int i = 0; i < rayCount; i++)
        {
            Vector2 dir = RayDirection(i);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, range, obstacleMask);
            if (hit.collider != null)
            {
                ObstacleAhead = true;
                NearestObstacleDistance = Mathf.Min(NearestObstacleDistance, hit.distance);
            }
        }
    }

    public bool HasLineOfSight(Vector2 targetPosition)
    {
        return Physics2D.Linecast(transform.position, targetPosition, obstacleMask).collider == null;
    }

    private Vector2 RayDirection(int index)
    {
        float t = rayCount == 1 ? 0.5f : (float)index / (rayCount - 1);
        float angle = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);
        return Quaternion.Euler(0f, 0f, angle) * _forward;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 dir = RayDirection(i);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, range, obstacleMask);
            Gizmos.color = hit.collider != null ? Color.red : Color.green;
            float length = hit.collider != null ? hit.distance : range;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * length);
        }
    }
}
