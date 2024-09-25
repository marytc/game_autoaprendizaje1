using UnityEngine;
using System.Collections;


[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    // Máscara de capas para detectar colisiones
    public LayerMask collisionMask;

    // Ancho del "skin" para el cálculo de raycasts
    public const float skinWidth = .015f;

    // Cantidad de rayos horizontales y verticales
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    // Espaciado entre rayos horizontales y verticales
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    // Referencia al BoxCollider2D del objeto
    [HideInInspector]
    public BoxCollider2D collider;

    // Estructura para almacenar los origenes de los raycasts
    public RaycastOrigins raycastOrigins;

    // Metodo que se llama al inicio
    public virtual void Start()
    {
        // Obtiene el BoxCollider2D del objeto
        collider = GetComponent<BoxCollider2D>();
        // Calcula el espaciado de los rayos
        CalculateRaySpacing();
    }

    // Actualiza las posiciones de los origenes de los raycasts
    public void UpdateRaycastOrigins()
    {
        // Obtiene los limites del collider
        Bounds bounds = collider.bounds;
        // Expande los límites para tener en cuenta el skinWidth
        bounds.Expand(skinWidth * -2);

        // Establece las posiciones de los orígenes de los raycasts
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // Calcula el espaciado entre los rayos
    public void CalculateRaySpacing()
    {
        // Obtiene los límites del collider
        Bounds bounds = collider.bounds;
        // Expande los límites para tener en cuenta el skinWidth
        bounds.Expand(skinWidth * -2);

        // Asegura que haya al menos 2 rayos en cada dirección
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calcula el espaciado entre los rayos horizontales y verticales
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    // Estructura para almacenar las posiciones de los orígenes de los raycasts
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight; // Esquinas superiores
        public Vector2 bottomLeft, bottomRight; // Esquinas inferiores
    }
}
