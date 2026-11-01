using System;

/// <summary>
/// Durumlar arası geçişi yöneten sade C# sınıfı (MonoBehaviour değil).
/// Geçiş kuralları durumların içindedir; bu sınıf sadece geçişi uygular.
/// </summary>
public class CompanionStateMachine
{
    public ICompanionState Current { get; private set; }

    /// <summary>(önceki, yeni) — loglama ve UI için.</summary>
    public event Action<ICompanionState, ICompanionState> StateChanged;

    public void ChangeState(ICompanionState next)
    {
        if (next == null || next == Current) return;

        ICompanionState previous = Current;
        previous?.Exit();
        Current = next;
        StateChanged?.Invoke(previous, next);
        Current.Enter();
    }

    public void Tick() => Current?.Tick();
}
