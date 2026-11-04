using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyun akışının sonu: bitiş ekranını gösterir. R: yeniden başlat, Esc: çık.
/// </summary>
public class GameManager : MonoBehaviour
{
    public bool IsGameOver { get; private set; }
    private string _message;

    public void EndGame(string message)
    {
        if (IsGameOver) return;
        IsGameOver = true;
        _message = message;
        Time.timeScale = 0f;
        Debug.Log("[Game] Oyun bitti: " + message);
    }

    private void Update()
    {
        if (!IsGameOver) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void OnGUI()
    {
        if (!IsGameOver) return;
        var style = new GUIStyle(GUI.skin.box) { fontSize = 26, alignment = TextAnchor.MiddleCenter, wordWrap = true, richText = true };
        var rect = new Rect(Screen.width / 2f - 260, Screen.height / 2f - 90, 520, 180);
        GUI.Box(rect, GUIContent.none);
        GUI.Box(rect, $"{_message}\n\n<size=16>[R] Yeniden başla   [Esc] Çık</size>", style);
    }
}
