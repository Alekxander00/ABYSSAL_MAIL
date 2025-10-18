using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation References")]
    public Animator animator;

    private PlayerMovement movement;
    private Rigidbody2D rb; // Referencia directa al Rigidbody2D
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>(); // Obtener Rigidbody2D
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Usar rb.velocity directamente en lugar del m�todo de PlayerMovement
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;
        animator.SetBool(IsMoving, isMoving);
        animator.SetFloat(MoveSpeed, rb.linearVelocity.magnitude);
    }
}