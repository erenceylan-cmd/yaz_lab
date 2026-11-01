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

    public NavMeshAgent Agent { get; private set; }
    public RaycastSensor Sensor { get; private set; }
    public CompanionStateMachine StateMachine { get; private set; }
    public IInteractable CurrentTarget { get; private set; }

    public Transform Player => player;
    public float FollowStartDistance => followStartDistance;
    public float FollowStopDistance => followStopDistance;
    public float InteractDuration => interactDuration;
    public float StuckTimeout => stuckTimeout;

    // Durumlar bir kez oluşturulur ve tekrar kullanılır.
    public IdleState Idle { get; private set; }
    public FollowState Follow { get; private set; }
    public GoToTargetState GoToTarget { get; private set; }
    public InteractState Interact { get; private set; }

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

        // Zaten bir hedefe gidiyorsa, yeni hedef için yolu baştan planlasın.
        if (StateMachine.Current == GoToTarget) StateMachine.ChangeState(Idle);

        CurrentTarget = target;
        StateMachine.ChangeState(GoToTarget);
        return StateMachine.Current == GoToTarget; // yol bulunamazsa Follow'a düşmüş olur
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
