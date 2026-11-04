using UnityEngine;

/// <summary>Oyuncu bu alana girince oyun biter (kapının arkasına konur).</summary>
[RequireComponent(typeof(Collider2D))]
public class GameEndZone : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private string endMessage = "Tebrikler! Kaleden çıktın.";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null && gameManager != null)
            gameManager.EndGame(endMessage);
    }
}
