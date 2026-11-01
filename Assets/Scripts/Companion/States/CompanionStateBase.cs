/// <summary>
/// Tüm durumların ortak tabanı. Her durum, kontrol ettiği CompanionAgent'a erişir.
/// </summary>
public abstract class CompanionStateBase : ICompanionState
{
    protected readonly CompanionAgent Companion;

    protected CompanionStateBase(CompanionAgent companion)
    {
        Companion = companion;
    }

    public abstract string Name { get; }
    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}
