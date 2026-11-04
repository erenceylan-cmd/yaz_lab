using System.Threading;
using UnityEngine;

/// <summary>
/// Kapıyı koruyan NPC. Bileşenleri birbirine bağlayan "composition root":
///   OllamaClient (ILLMClient) + NPCResponseParser + NPCDialogueController + IDialogueView + Door
/// Oyuncu yakına gelince [T] ile konuşma açılır, [Esc] ile kapanır.
/// Model door_open = true döndürürse kapı açılır.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GuardNPC : MonoBehaviour
{
    [Header("Kişilik")]
    [SerializeField] private NPCPersona persona = new NPCPersona();

    [Header("LLM")]
    [SerializeField] private OllamaConfig ollama = new OllamaConfig();

    [Header("Bağlantılar")]
    [SerializeField] private Door guardedDoor;
    [SerializeField] private DialogueUI dialogueView;
    [Tooltip("Konuşurken kapatılacak bileşenler (PlayerController, CompanionCommandInput).")]
    [SerializeField] private Behaviour[] disableWhileTalking;

    [Header("Etkileşim")]
    [SerializeField] private KeyCode talkKey = KeyCode.T;

    private OllamaClient _client;
    private NPCDialogueController _dialogue;
    private IDialogueView _view;
    private CancellationTokenSource _lifetimeCts;
    private bool _playerInRange;
    private bool _talking;

    private void Awake()
    {
        _lifetimeCts = new CancellationTokenSource();
        _client = new OllamaClient(ollama);
        _dialogue = new NPCDialogueController(_client, new NPCResponseParser(), persona);
        _view = dialogueView;
    }

    private async void Start()
    {
        if (_view == null)
        {
            Debug.LogError("[GuardNPC] Dialogue View atanmamış!");
            enabled = false;
            return;
        }

        _view.MessageSubmitted += OnPlayerMessage;
        _view.CloseRequested += EndConversation;

        // Açılışta sunucu kontrolü: kapalıysa oyun yine devam eder, sadece uyarı gösterilir.
        bool available = await _client.IsAvailableAsync(_lifetimeCts.Token);
        if (this == null) return;
        _view.SetServerStatus(available
            ? $"LLM: {ollama.model} hazır"
            : $"LLM: bağlantı yok ({ollama.model}) — Ollama'yı başlatın");
    }

    private void Update()
    {
        if (_playerInRange && !_talking && Input.GetKeyDown(talkKey))
            BeginConversation();
    }

    private void BeginConversation()
    {
        _talking = true;
        SetPlayerControls(false);
        _view.Open(persona.displayName);
        _view.AppendNpcMessage(persona.displayName, persona.greeting, "neutral");
    }

    private void EndConversation()
    {
        if (!_talking) return;
        _talking = false;
        _view.Close();
        SetPlayerControls(true);
    }

    private async void OnPlayerMessage(string text)
    {
        if (_dialogue.IsBusy) return;

        _view.AppendPlayerMessage(text);
        _view.SetThinking(true);

        DialogueOutcome outcome = await _dialogue.SendAsync(text, _lifetimeCts.Token);

        if (this == null) return; // sahne kapandıysa
        _view.SetThinking(false);

        if (!outcome.Success)
        {
            _view.AppendSystemMessage(outcome.ErrorMessage);
            return;
        }

        NPCResponse r = outcome.Response;
        _view.AppendNpcMessage(persona.displayName, r.reply, r.mood);

        if (r.door_open && guardedDoor != null && !guardedDoor.IsOpen)
        {
            guardedDoor.Open();
            _view.AppendSystemMessage("Kapı açıldı! [Esc] ile konuşmayı bitirip ilerleyebilirsin.");
        }
    }

    private void SetPlayerControls(bool enabledState)
    {
        if (disableWhileTalking == null) return;
        foreach (Behaviour b in disableWhileTalking)
            if (b != null) b.enabled = enabledState;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null) _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null) _playerInRange = false;
    }

    private void OnGUI()
    {
        if (!_playerInRange || _talking || Camera.main == null) return;
        Vector3 p = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.9f);
        var style = new GUIStyle(GUI.skin.box) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        GUI.Box(new Rect(p.x - 70, Screen.height - p.y - 14, 140, 28), $"[{talkKey}] Konuş", style);
    }

    private void OnDestroy()
    {
        if (_view != null)
        {
            _view.MessageSubmitted -= OnPlayerMessage;
            _view.CloseRequested -= EndConversation;
        }
        _lifetimeCts?.Cancel();
        _lifetimeCts?.Dispose();
        _client?.Dispose();
    }
}
