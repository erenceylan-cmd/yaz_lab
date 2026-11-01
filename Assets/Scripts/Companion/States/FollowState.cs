using UnityEngine;

/// <summary>
/// Takip: NavMesh üzerinden oyuncuya yol bulur. Yeterince yaklaşınca Idle'a döner.
/// Hedef her karede değil, belirli aralıklarla güncellenir (performans).
/// </summary>
public class FollowState : CompanionStateBase
{
    private const float RepathInterval = 0.25f;
    private float _repathTimer;

    public FollowState(CompanionAgent companion) : base(companion) { }

    public override string Name => "Follow";

    public override void Enter()
    {
        _repathTimer = 0f;
        Companion.MoveTo(Companion.Player.position);
    }

    public override void Tick()
    {
        if (Companion.DistanceToPlayer() <= Companion.FollowStopDistance)
        {
            Companion.StateMachine.ChangeState(Companion.Idle);
            return;
        }

        _repathTimer += Time.deltaTime;
        if (_repathTimer >= RepathInterval)
        {
            _repathTimer = 0f;
            Companion.MoveTo(Companion.Player.position);
        }
    }
}
