using UnityEngine;

/// <summary>
/// Oyuncu hareketi (WASD / ok tuşları). Diyalog sırasında bileşen kapatılınca karakter durur.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _input;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Update()
    {
        _input = new Vector2(Input.GetAxisRaw("Horizontal"),
                             Input.GetAxisRaw("Vertical")).normalized;
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _input * moveSpeed;
    }

    private void OnDisable()
    {
        _input = Vector2.zero;
        if (_rb != null) _rb.linearVelocity = Vector2.zero; // kayarak gitmesin
    }
}
