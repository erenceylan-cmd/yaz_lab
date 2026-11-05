using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IDialogueView'in IMGUI ile çizilen uygulaması (Canvas kurulumu gerektirmez).
/// Ekranın alt kısmında sohbet geçmişi, yazı kutusu ve "Düşünüyor..." göstergesi.
/// Enter: gönder, Esc: kapat.
/// </summary>
public class DialogueUI : MonoBehaviour, IDialogueView
{
    private const string InputControlName = "DialogueInput";
    private const int MaxInputLength = 300;

    public event Action<string> MessageSubmitted;
    public event Action CloseRequested;

    private readonly List<(string text, Color color)> _lines = new List<(string, Color)>();
    private string _npcName = "";
    private string _input = "";
    private string _serverStatus = "LLM: kontrol ediliyor...";
    private bool _open;
    private bool _thinking;
    private bool _focusInput;
    private Vector2 _scroll;
    private GUIStyle _lineStyle, _titleStyle, _statusStyle;

    public void Open(string npcName)
    {
        _npcName = npcName;
        _open = true;
        _focusInput = true;
    }

    public void Close()
    {
        _open = false;
        _input = "";
    }

    public void AppendPlayerMessage(string text) => AddLine($"Sen: {text}", new Color(0.6f, 0.85f, 1f));

    public void AppendNpcMessage(string npcName, string text, string mood) =>
        AddLine($"{npcName} ({MoodLabel(mood)}): {text}", MoodColor(mood));

    public void AppendSystemMessage(string text) => AddLine($"! {text}", new Color(1f, 0.8f, 0.3f));

    public void SetThinking(bool thinking)
    {
        _thinking = thinking;
        if (!thinking) _focusInput = true;
    }

    public void SetServerStatus(string status) => _serverStatus = status;

    private void AddLine(string text, Color color)
    {
        _lines.Add((text, color));
        _scroll.y = float.MaxValue; // en alta kaydır
    }

    private void OnGUI()
    {
        DrawStatus();
        if (!_open) return;

        EnsureStyles();

        float w = Mathf.Min(900f, Screen.width - 40f);
        float h = Mathf.Min(320f, Screen.height * 0.45f);
        var panel = new Rect((Screen.width - w) / 2f, Screen.height - h - 20f, w, h);
        GUI.Box(panel, GUIContent.none);
        GUI.Box(panel, GUIContent.none); // daha koyu arka plan

        GUILayout.BeginArea(new Rect(panel.x + 12, panel.y + 8, panel.width - 24, panel.height - 16));
        GUILayout.Label($"{_npcName} ile konuşma   —   Enter: gönder, Esc: kapat", _titleStyle);

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        foreach (var (text, color) in _lines)
        {
            _lineStyle.normal.textColor = color;
            GUILayout.Label(text, _lineStyle);
        }
        if (_thinking)
        {
            _lineStyle.normal.textColor = Color.gray;
            int dots = 1 + (int)(Time.unscaledTime * 3f) % 3;
            GUILayout.Label($"{_npcName} düşünüyor" + new string('.', dots), _lineStyle);
        }
        GUILayout.EndScrollView();

        HandleKeys();

        GUILayout.BeginHorizontal();
        GUI.enabled = !_thinking;
        GUI.SetNextControlName(InputControlName);
        _input = GUILayout.TextField(_input, MaxInputLength, GUILayout.Height(28));
        if (GUILayout.Button(_thinking ? "Bekleniyor..." : "Gönder", GUILayout.Width(120), GUILayout.Height(28)))
            Submit();
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        if (_focusInput && !_thinking)
        {
            GUI.FocusControl(InputControlName);
            _focusInput = false;
        }
    }

    private void HandleKeys()
    {
        Event e = Event.current;
        if (e.type != EventType.KeyDown) return;

        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            Submit();
            e.Use();
        }
        else if (e.keyCode == KeyCode.Escape)
        {
            CloseRequested?.Invoke();
            e.Use();
        }
    }

    private void Submit()
    {
        if (_thinking || string.IsNullOrWhiteSpace(_input)) return;
        string text = _input.Trim();
        _input = "";
        MessageSubmitted?.Invoke(text);
    }

    private void DrawStatus()
    {
        _statusStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 12 };
        GUI.Label(new Rect(10, 8, 500, 22), _serverStatus, _statusStyle);
    }

    private void EnsureStyles()
    {
        _lineStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, richText = false };
        _titleStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
    }

    private static string MoodLabel(string mood) => mood switch
    {
        "friendly" => "dostça",
        "suspicious" => "şüpheli",
        "angry" => "kızgın",
        _ => "nötr"
    };

    private static Color MoodColor(string mood) => mood switch
    {
        "friendly" => new Color(0.55f, 1f, 0.55f),
        "suspicious" => new Color(1f, 0.9f, 0.4f),
        "angry" => new Color(1f, 0.45f, 0.45f),
        _ => Color.white
    };
}
