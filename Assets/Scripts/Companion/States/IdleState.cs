/// <summary>
/// Bekleme: oyuncuya yeterince yakınken durur. Oyuncu uzaklaşırsa Follow'a geçer.
/// </summary>
public class IdleState : CompanionStateBase
{
    public IdleState(CompanionAgent companion) : base(companion) { }

    public override string Name => "Idle";

    public override void Enter() => Companion.StopMoving();

    public override void Tick()
    {
        if (Companion.DistanceToPlayer() > Companion.FollowStartDistance)
            Companion.StateMachine.ChangeState(Companion.Follow);
    }
}
