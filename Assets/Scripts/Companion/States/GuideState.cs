using UnityEngine;

/// <summary>
/// Rehberlik: companion bir IHintProvider'ın gösterdiği konuma (örn. sıradaki doğru plaka) gider
/// ve orada bekleyerek oyuncuya işaret eder. İpucu değişince (oyuncu doğru/yanlış bastı)
/// yeni konuma NavMesh ile yol bulur. İpucu kalmayınca (bulmaca çözüldü) Follow'a döner.
/// </summary>
public class GuideState : CompanionStateBase
{
    private const float RecheckInterval = 0.5f;

    private IHintProvider _provider;
    private Vector3 _lastHint;
    private float _recheckTimer;

    public GuideState(CompanionAgent companion) : base(companion) { }

    public override string Name => "Guide";

    public override void Enter()
    {
        _provider = Companion.HintProvider;
        if (_provider == null) return;

        _provider.HintChanged += OnHintChanged;
        GoToHint();
    }

    public override void Exit()
    {
        if (_provider != null) _provider.HintChanged -= OnHintChanged;
        _provider = null;
    }

    public override void Tick()
    {
        if (_provider == null || !_provider.HasHint)
        {
            Companion.ClearHint();
            Companion.StateMachine.ChangeState(Companion.Follow);
            return;
        }

        // Güvenlik ağı: olay kaçırılırsa da hedef güncel kalsın.
        _recheckTimer += Time.deltaTime;
        if (_recheckTimer >= RecheckInterval)
        {
            _recheckTimer = 0f;
            if (Vector2.Distance(_provider.HintPosition, _lastHint) > 0.1f) GoToHint();
        }
    }

    private void OnHintChanged()
    {
        if (_provider != null && _provider.HasHint) GoToHint();
    }

    private void GoToHint()
    {
        _lastHint = _provider.HintPosition;
        Companion.MoveTo(_lastHint);
        Debug.Log("[Companion] Sıradaki doğru plakayı gösteriyor.");
    }
}
