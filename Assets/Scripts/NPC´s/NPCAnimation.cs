using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Para NPCs que se muevan (opcional)
    public void SetMoving(bool moving)
    {
        animator.SetBool("IsMoving", moving);
    }
}