using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string interactionText = "Interactuar";
    public bool canInteractMultipleTimes = true;

    protected bool hasBeenInteracted = false;

    public abstract void Interact(GameObject player);

    public virtual bool CanInteract()
    {
        return canInteractMultipleTimes || !hasBeenInteracted;
    }

    protected virtual void OnInteractSuccess()
    {
        hasBeenInteracted = true;
        Debug.Log($"Interacción exitosa con: {gameObject.name}");
    }
}