using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    [Header("Configurações do Gizmo")]
    public Color gizmoColor = Color.red;
    public float radius = 10f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius); // Desenha uma esfera de gizmo
    }
}
