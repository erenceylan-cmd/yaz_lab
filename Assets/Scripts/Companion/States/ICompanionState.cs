/// <summary>
/// Companion durum makinesindeki her durumun uyması gereken sözleşme (State Pattern).
/// </summary>
public interface ICompanionState
{
    string Name { get; }
    void Enter();   // Duruma girerken bir kez çalışır
    void Tick();    // Durum aktifken her karede çalışır
    void Exit();    // Durumdan çıkarken bir kez çalışır
}
