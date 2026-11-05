using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Yol bekleme: hedefe giden yol şu an kapalı (örn. tünel kapısı kapalı).
/// Companion hedefe ulaşabileceği en yakın noktaya gider ve düzenli aralıklarla yolu yeniden dener.
/// - Yol açılırsa: GoToTarget'a döner.
/// - Süre dolarsa: görevi bırakır, Follow'a döner.
/// </summary>
public class WaitForPathState : CompanionStateBase
{
    private const float CheckInterval = 0.25f;

    private float _waitTimer;
    private float _checkTimer;

    public WaitForPathState(CompanionAgent companion) : base(companion) { }

    public override string Name => "WaitForPath";

    public override void Enter()
    {
        _waitTimer = 0f;
        _checkTimer = 0f;
        Debug.Log("[Companion] Hedefe giden yol kapalı. En yakın noktada yolun açılması bekleniyor.");

        // Kısmi yol: hedefe ulaşılabilecek en yakın noktaya git (örn. kapalı kapının önü).
        if (Companion.CurrentTarget != null)
            Companion.MoveTo(Companion.CurrentTarget.InteractionPoint.position);
    }

    public override void Tick()
    {
        IInteractable target = Companion.CurrentTarget;
        if (target == null || !target.CanInteract)
        {
            Companion.ClearTarget();
            Companion.StateMachine.ChangeState(Companion.Follow);
            return;
        }

        _waitTimer += Time.deltaTime;
        if (_waitTimer >= Companion.WaitForPathTimeout)
        {
            Debug.LogWarning($"[Companion] {Companion.WaitForPathTimeout:0} sn boyunca yol açılmadı. Görev bırakıldı.");
            Companion.ClearTarget();
            Companion.StateMachine.ChangeState(Companion.Follow);
            return;
        }

        _checkTimer += Time.deltaTime;
        if (_checkTimer < CheckInterval) return;
        _checkTimer = 0f;

        if (Companion.TryGetPathToTarget(out NavMeshPath _))
        {
            Debug.Log("[Companion] Yol açıldı, hedefe devam ediliyor.");
            Companion.StateMachine.ChangeState(Companion.GoToTarget);
        }
    }
}
