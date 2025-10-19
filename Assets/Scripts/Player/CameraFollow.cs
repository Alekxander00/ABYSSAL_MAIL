using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // El jugador

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 0, -10); // Offset estándar para cámara 2D
    public float smoothSpeed = 5f; // Velocidad de suavizado

    [Header("Rotation Settings")]
    public bool followRotation = false; // Si quieres que la cámara rote con el player
    public float rotationSmoothSpeed = 2f; // Velocidad de suavizado para rotación

    [Header("Bounds (Opcional)")]
    public bool useBounds = false;
    public Vector2 minBounds = new Vector2(-10, -10);
    public Vector2 maxBounds = new Vector2(10, 10);

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Si no hay target asignado, buscar automáticamente al player
        if (target == null)
        {
            FindPlayer();
        }

        // Posicionar la cámara inmediatamente en el player al inicio
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            if (useBounds)
            {
                targetPosition = GetBoundedPosition(targetPosition);
            }
            transform.position = targetPosition;
        }

        Debug.Log("📷 Cámara inicializada - Modo: " + (followRotation ? "Posición + Rotación" : "Solo Posición"));
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }

        FollowTarget();
    }

    private void FollowTarget()
    {
        // Seguimiento de posición
        Vector3 targetPosition = target.position + offset;

        // Aplicar límites si están activados
        if (useBounds)
        {
            targetPosition = GetBoundedPosition(targetPosition);
        }

        // Suavizado de posición
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / smoothSpeed);

        // Seguimiento de rotación (opcional)
        if (followRotation)
        {
            Quaternion targetRotation = target.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetBoundedPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        return position;
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("✅ Cámara encontró automáticamente al Player");
        }
        else
        {
            Debug.LogWarning("⚠️ Cámara no puede encontrar el Player. Asigna manualmente en el Inspector.");
        }
    }

    // Método para cambiar el target en tiempo de ejecución
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("🎯 Nuevo target asignado a la cámara: " + newTarget.name);
    }

    // Método para cambiar dinámicamente el offset
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        Debug.Log("📐 Offset de cámara cambiado a: " + newOffset);
    }

    // Método para activar/desactivar rotación en tiempo de ejecución
    public void SetFollowRotation(bool enable)
    {
        followRotation = enable;
        Debug.Log("🔄 Seguimiento de rotación: " + (enable ? "ACTIVADO" : "DESACTIVADO"));
    }

    // Para debug visual de los límites en el editor
    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) * 0.5f,
                (minBounds.y + maxBounds.y) * 0.5f,
                transform.position.z
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0.1f
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}