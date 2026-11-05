using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Companion'ın ana bileşeni. NavMeshAgent, sensör ve durum makinesini bir araya getirir.
/// Davranış kararları durum sınıflarındadır; bu sınıf onlara ortak yardımcıları sunar.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RaycastSensor))]
public class CompanionAgent : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform player;

    [Header("Takip")]
    [SerializeField] private float followStartDistance = 2.5f;
    [SerializeField] private float followStopDistance = 1.5f;

    [Header("Görev")]
    [SerializeField] private float arriveThreshold = 0.15f;
    [SerializeField] private float interactDuration = 0.6f;
    [SerializeField] private float stuckTimeout = 2f;
    [Tooltip("Yol kapalıyken (örn. kapı kapalı) en fazla kaç saniye beklenir.")]
    [SerializeField] private float waitForPathTimeout = 20f;

    public NavMeshAgent Agent { get; private set; }
    public RaycastSensor Sensor { get; private set; }
    public CompanionStateMachine StateMachine { get; private set; }
    public IInteractable CurrentTarget { get; private set; }

    public Transform Player => player;
    public float FollowStartDistance => followStartDistance;
    public float FollowStopDistance => followStopDistance;
    public float InteractDuration => interactDuration;
    public float StuckTimeout => stuckTimeout;
    public float WaitForPathTimeout => waitForPathTimeout;

    // Durumlar bir kez oluşturulur ve tekrar kullanılır.
    public IdleState Idle { get; private set; }
    public FollowState Follow { get; private set; }
    public GoToTargetState GoToTarget { get; private set; }
    public InteractState Interact { get; private set; }
    public WaitForPathState WaitForPath { get; private set; }

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false; // 2D: sprite dönmesin
        Agent.updateUpAxis = false;   // 2D: XY düzleminde kalsın

        Sensor = GetComponent<RaycastSensor>();

        StateMachine = new CompanionStateMachine();
        StateMachine.StateChanged += (previous, next) =>
            Debug.Log($"[Companion] Durum: {previous?.Name ?? "-"} -> {next.Name}");

        Idle = new IdleState(this);
        Follow = new FollowState(this);
        GoToTarget = new GoToTargetState(this);
        Interact = new InteractState(this);
        WaitForPath = new WaitForPathState(this);
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("[Companion] Player referansı atanmadı! Inspector'dan Player'ı sürükleyin.");
            enabled = false;
            return;
        }

        SnapToNavMesh();
        StateMachine.ChangeState(Idle);
    }

    private void Update()
    {
        Sensor.Scan(Agent.velocity);
        StateMachine.Tick();
    }

    // ---------- Dışarıdan gelen komutlar ----------

    /// <summary>Companion'a bir nesneyle etkileşim görevi verir. Kabul edilirse true döner.</summary>
    public bool TryCommandInteract(IInteractable target)
    {
        if (target == null || !target.CanInteract) return false;

        // Zaten bir hedefe gidiyorsa / bekliyorsa, yeni hedef için yolu baştan planlasın.
        if (StateMachine.Current == GoToTarget || StateMachine.Current == WaitForPath)
            StateMachine.ChangeState(Idle);

        CurrentTarget = target;
        StateMachine.ChangeState(GoToTarget);
        // Yol kapalıysa WaitForPath'te bekler; o da kabul edilmiş bir görevdir.
        return StateMachine.Current == GoToTarget || StateMachine.Current == WaitForPath;
    }

    public void CommandFollow()
    {
        ClearTarget();
        StateMachine.ChangeState(Follow);
    }

    // ---------- Durumların kullandığı yardımcılar ----------

    public void MoveTo(Vector3 position)
    {
        Agent.isStopped = false;
        Agent.SetDestination(position);
    }

    public void StopMoving()
    {
        if (Agent.isOnNavMesh) Agent.ResetPath();
    }

    public bool HasArrived()
    {
        if (Agent.pathPending) return false;
        return Vector2.Distance(transform.position, Agent.destination) <= Agent.stoppingDistance + arriveThreshold;
    }

    /// <summary>
    /// Mevcut hedefe (CurrentTarget) TAM bir NavMesh yolu var mı? Ajanın areaMask'i dikkate alınır,
    /// yani kapalı kapıların alanları yol hesabına girmez.
    /// </summary>
    public bool TryGetPathToTarget(out NavMeshPath path)
    {
        path = null;
        if (CurrentTarget == null || !Agent.isOnNavMesh) return false;

        Vector3 point = CurrentTarget.InteractionPoint.position;
        // Hedef nokta NavMesh'in biraz dışında olabilir (duvar dibi): en yakın yürünebilir noktayı bul.
        if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return false;

        path = new NavMeshPath();
        return Agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    public float DistanceToPlayer() => Vector2.Distance(transform.position, player.position);

    public void ClearTarget() => CurrentTarget = null;

    private void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            Agent.Warp(hit.position);
        else
            Debug.LogError("[Companion] Başlangıç noktası NavMesh üzerinde değil! NavMesh'i bake ettiniz mi?");
    }
}
