using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Şifreli plaka bulmacası: her oyunda rastgele bir şifre üretilir (örn. 7-2-9-4).
/// Oyuncu plakalara doğru sırayla basarsa kapı açılır; yanlış plakada ilerleme sıfırlanır.
/// IHintProvider olarak companion'a "sıradaki doğru plakanın" konumunu verir.
/// </summary>
public class SequencePuzzle : MonoBehaviour, IHintProvider
{
    [SerializeField] private Door door;
    [SerializeField, Range(2, 6)] private int codeLength = 4;
    [SerializeField] private Color correctColor = new Color(0.3f, 0.9f, 0.3f);
    [SerializeField] private Color wrongColor = new Color(0.95f, 0.25f, 0.25f);
    [SerializeField] private Color solvedColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private float wrongFlashSeconds = 0.6f;
    [SerializeField] private KeyCode hintKeyLabel = KeyCode.Q;

    private NumberPlate[] _plates;
    private int[] _code;
    private int _progress;
    private string _message;
    private float _messageUntil;
    private Transform _player;

    public bool IsSolved { get; private set; }
    public bool HasHint => !IsSolved && _code != null && _code.Length > 0;
    public Vector3 HintPosition => PlateFor(_code[_progress]).transform.position;
    public event Action HintChanged;

    private void Awake()
    {
        _plates = FindObjectsByType<NumberPlate>(FindObjectsSortMode.None);
        if (door == null) door = GameObject.Find("PuzzleDoor")?.GetComponent<Door>();
        _player = FindFirstObjectByType<PlayerController>()?.transform;

        if (_plates.Length == 0)
        {
            Debug.LogError("[Puzzle] Sahnede NumberPlate yok.");
            enabled = false;
            return;
        }

        foreach (NumberPlate plate in _plates) plate.Pressed += OnPlatePressed;
        GenerateCode();
    }

    private void GenerateCode()
    {
        int[] numbers = _plates.Select(p => p.Number).Distinct().ToArray();
        int length = Mathf.Min(codeLength, numbers.Length);
        _code = new int[length];

        var rng = new System.Random();
        for (int i = 0; i < length; i++)
        {
            int n;
            do { n = numbers[rng.Next(numbers.Length)]; }
            while (i > 0 && n == _code[i - 1]); // arka arkaya aynı plaka olmasın
            _code[i] = n;
        }
        _progress = 0;
        Debug.Log("[Puzzle] Bu oyunun şifresi: " + string.Join("-", _code));
    }

    private void OnPlatePressed(NumberPlate plate)
    {
        if (IsSolved) return;

        if (plate.Number == _code[_progress])
        {
            plate.SetColor(correctColor);
            _progress++;

            if (_progress >= _code.Length)
            {
                Solve();
            }
            else
            {
                ShowMessage($"Doğru! ({_progress}/{_code.Length})", 1.5f);
                HintChanged?.Invoke();
            }
        }
        else
        {
            Debug.Log($"[Puzzle] Yanlış plaka: {plate.Number} (beklenen {_code[_progress]}). Sıfırlandı.");
            _progress = 0;
            foreach (NumberPlate p in _plates) p.ResetColor();
            StartCoroutine(FlashWrong(plate));
            ShowMessage("Yanlış plaka! Baştan başla.", 2f);
            HintChanged?.Invoke();
        }
    }

    private IEnumerator FlashWrong(NumberPlate plate)
    {
        plate.SetColor(wrongColor);
        yield return new WaitForSeconds(wrongFlashSeconds);
        if (!IsSolved && _progress == 0) plate.ResetColor();
    }

    private void Solve()
    {
        IsSolved = true;
        foreach (NumberPlate p in _plates) p.SetColor(solvedColor);
        if (door != null) door.Open();
        ShowMessage("Şifre doğru! Kapı açıldı.", 3f);
        Debug.Log("[Puzzle] Bulmaca çözüldü.");
        HintChanged?.Invoke();
    }

    private NumberPlate PlateFor(int number) => _plates.First(p => p.Number == number);

    private void ShowMessage(string text, float seconds)
    {
        _message = text;
        _messageUntil = Time.time + seconds;
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleCenter };

        if (Time.time < _messageUntil)
            GUI.Box(new Rect(Screen.width / 2f - 170, 40, 340, 32), _message, style);

        if (!IsSolved && _player != null && IsPlayerNearPuzzle())
        {
            string dots = string.Concat(Enumerable.Range(0, _code.Length).Select(i => i < _progress ? "● " : "○ "));
            GUI.Box(new Rect(Screen.width / 2f - 170, 78, 340, 28),
                    $"Şifre: {dots}  [{hintKeyLabel}] Companion'dan ipucu", style);
        }
    }

    private bool IsPlayerNearPuzzle()
    {
        Vector3 center = Vector3.zero;
        foreach (NumberPlate p in _plates) center += p.transform.position;
        center /= _plates.Length;
        return Vector2.Distance(_player.position, center) < 5f;
    }

    private void OnDestroy()
    {
        if (_plates == null) return;
        foreach (NumberPlate plate in _plates) if (plate != null) plate.Pressed -= OnPlatePressed;
    }
}
