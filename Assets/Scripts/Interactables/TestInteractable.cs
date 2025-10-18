using UnityEngine;

public class TestInteractable : Interactable
{
    [Header("Test Settings")]
    public string testMessage = "¡Funciona!";

    public override void Interact(GameObject player)
    {
        Debug.Log($"🔹 {testMessage} - Interactuando con: {gameObject.name}");
        Debug.Log($"🔹 Jugador que interactúa: {player.name}");

        // Opcional: cambiar color para feedback visual
        GetComponent<SpriteRenderer>().color = Color.green;

        OnInteractSuccess();
    }
}