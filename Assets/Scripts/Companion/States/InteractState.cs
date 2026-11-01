using UnityEngine;

/// <summary>
/// Etkileşim: hedefe varınca durur, kısa bir süre "çalışır", Raycast ile hedefi
/// gerçekten gördüğünü doğrular ve etkileşimi tetikler. Sonra takibe döner.
/// </summary>
public class InteractState : CompanionStateBase
{
    private float _timer;

    public InteractState(CompanionAgent companion) : base(companion) { }

    public override string Name => "Interact";

    public override void Enter()
    {
        _timer = 0f;
        Companion.StopMoving();
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer < Companion.InteractDuration) return;

        IInteractable target = Companion.CurrentTarget;

        if (target != null && target.CanInteract)
        {
            Vector2 targetPos = target.InteractionPoint.position;
            if (Companion.Sensor.HasLineOfSight(targetPos))
            {
                target.Interact(Companion.gameObject);
                Debug.Log("[Companion] Etkileşim tamamlandı.");
            }
            else
            {
                Debug.LogWarning("[Companion] Hedef görüş hattında değil, etkileşim yapılamadı.");
            }
        }

        Companion.ClearTarget();
        Companion.StateMachine.ChangeState(Companion.Follow);
    }
}
