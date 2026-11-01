using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hedefe gitme: NavMesh ile etkileşim noktasına tam (complete) bir yol hesaplar ve izler.
/// - Yol yoksa: uyarı verir, Follow'a döner (oyun çökmez).
/// - Takılırsa: Raycast sensörü engel görüyorsa yolu yeniden hesaplar; sınırlı deneme.
/// - Varınca: Interact durumuna geçer.
/// </summary>
public class GoToTargetState : CompanionStateBase
{
    private const int MaxRepathAttempts = 3;

    private float _stuckTimer;
    private int _repathAttempts;

    public GoToTargetState(CompanionAgent companion) : base(companion) { }

    public override string Name => "GoToTarget";

    public override void Enter()
    {
        _stuckTimer = 0f;
        _repathAttempts = 0;

        if (!TryPlanPath())
            Abort("Hedefe giden geçerli bir yol bulunamadı.");
    }

    public override void Tick()
    {
        IInteractable target = Companion.CurrentTarget;
        if (target == null || !target.CanInteract)
        {
            Abort("Hedef artık geçerli değil.");
            return;
        }

        if (Companion.HasArrived())
        {
            Companion.StateMachine.ChangeState(Companion.Interact);
            return;
        }

        // Takılma tespiti: yol hazır ama ajan neredeyse hiç hareket etmiyor.
        bool barelyMoving = Companion.Agent.velocity.sqrMagnitude < 0.01f;
        _stuckTimer = (!Companion.Agent.pathPending && barelyMoving) ? _stuckTimer + Time.deltaTime : 0f;

        if (_stuckTimer >= Companion.StuckTimeout)
        {
            _stuckTimer = 0f;
            _repathAttempts++;

            string reason = Companion.Sensor.ObstacleAhead ? "önde engel var" : "hareket yok";
            Debug.LogWarning($"[Companion] Takıldı ({reason}). Yol yeniden hesaplanıyor ({_repathAttempts}/{MaxRepathAttempts}).");

            if (_repathAttempts > MaxRepathAttempts || !TryPlanPath())
                Abort("Takılma çözülemedi.");
        }
    }

    private bool TryPlanPath()
    {
        Vector3 point = Companion.CurrentTarget.InteractionPoint.position;

        // Hedef nokta NavMesh'in biraz dışında olabilir (duvar dibi): en yakın yürünebilir noktayı bul.
        if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return false;

        var path = new NavMeshPath();
        point = hit.position;

        if (!Companion.Agent.CalculatePath(point, path) || path.status != NavMeshPathStatus.PathComplete)
            return false;

        Companion.Agent.SetPath(path);
        return true;
    }

    private void Abort(string reason)
    {
        Debug.LogWarning($"[Companion] Görev iptal: {reason} Oyuncuyu takibe dönülüyor.");
        Companion.ClearTarget();
        Companion.StateMachine.ChangeState(Companion.Follow);
    }
}
