using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hedefe gitme: NavMesh ile etkileşim noktasına tam (complete) bir yol hesaplar ve izler.
/// - Yol kapalıysa (örn. kapı kapalı): WaitForPath durumuna geçer.
/// - Yol sırasında kapanırsa (yol "partial" olursa): WaitForPath durumuna geçer.
/// - Takılırsa: Raycast sensörü ile sebebi teşhis eder, yolu yeniden hesaplar; sınırlı deneme.
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

        if (Companion.CurrentTarget == null)
        {
            Abort("Hedef yok.");
            return;
        }

        if (Companion.TryGetPathToTarget(out NavMeshPath path))
            Companion.Agent.SetPath(path);
        else
            Companion.StateMachine.ChangeState(Companion.WaitForPath);
    }

    public override void Tick()
    {
        IInteractable target = Companion.CurrentTarget;
        if (target == null || !target.CanInteract)
        {
            Abort("Hedef artık geçerli değil.");
            return;
        }

        NavMeshAgent agent = Companion.Agent;

        // Yol yürürken kapandıysa (kapı kapandı): tam yol yok, bekle.
        if (!agent.pathPending && agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            Companion.StateMachine.ChangeState(Companion.WaitForPath);
            return;
        }

        if (Companion.HasArrived())
        {
            Companion.StateMachine.ChangeState(Companion.Interact);
            return;
        }

        // Takılma tespiti: yol hazır ama ajan neredeyse hiç hareket etmiyor.
        bool barelyMoving = agent.velocity.sqrMagnitude < 0.01f;
        _stuckTimer = (!agent.pathPending && barelyMoving) ? _stuckTimer + Time.deltaTime : 0f;

        if (_stuckTimer >= Companion.StuckTimeout)
        {
            _stuckTimer = 0f;
            _repathAttempts++;

            string reason = Companion.Sensor.ObstacleAhead ? "önde engel var" : "hareket yok";
            Debug.LogWarning($"[Companion] Takıldı ({reason}). Yol yeniden hesaplanıyor ({_repathAttempts}/{MaxRepathAttempts}).");

            if (_repathAttempts > MaxRepathAttempts)
                Abort("Takılma çözülemedi.");
            else if (Companion.TryGetPathToTarget(out NavMeshPath path))
                agent.SetPath(path);
            else
                Companion.StateMachine.ChangeState(Companion.WaitForPath);
        }
    }

    private void Abort(string reason)
    {
        Debug.LogWarning($"[Companion] Görev iptal: {reason} Oyuncuyu takibe dönülüyor.");
        Companion.ClearTarget();
        Companion.StateMachine.ChangeState(Companion.Follow);
    }
}
