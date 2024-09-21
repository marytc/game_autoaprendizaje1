using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    public LayerMask collisionMask;     // Máscara de colisión para detectar qué objetos 

    const float skinWidth = .015f;      // Ancho del raycast
    public int horizontalRayCount = 4;  // Número de rayos horizontales
    public int verticalRayCount = 4;    // Número de rayos verticales

    float horizontalRaySpacing; // Espaciado entre rayos horizontales
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();  // Calcula el espaciado de los rayos
    }

    // Metodo para mover al jugador
    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();

        // Verifica si hay movimiento horizontal
        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);     // Gestiona colisiones Horizontales
        }
        // Verifica si hay movimiento vertical
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);       // Gestiona colisiones Verticales
        }

        transform.Translate(velocity);      // Mueve al jugador
    }

    // Gestiona colisiones Horizontales
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);              // Determina la dirección del movimiento horizontal
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;    // Longitud del rayo

        // muestra los rayos Horizontales
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;    // Establece el origen del rayo en función de la dirección del movimiento
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);       // Ajusta la posición del rayo
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);  // Lanza el rayo

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);    // Dibuja el rayo rojos para depuración

            // Si hay una colisión
            if (hit)
            {
                velocity.x = (hit.distance - skinWidth) * directionX;   // Calcula la velocidad
                rayLength = hit.distance;   // Actualiza la longitud del rayo

                collisions.left = directionX == -1; // Actualiza el estado de colisión
                collisions.right = directionX == 1; // Actualiza el estado de colisión
            }
        }
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);  // Determina la dirección del movimiento
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;    // Longitud del rayo

        // genera de rayos verticales
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;    // Calcula el origen del rayo en función de la dirección del movimiento
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x); // Ajusta la posición del rayo
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);  // Dibuja los rayos rojos para depuración

            //Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            // Si hay una colisión
            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;   // Calcula la velocidad
                rayLength = hit.distance;   // Actualiza la longitud del rayo

                collisions.below = directionY == -1;    // Actualiza el estado de colisión
                collisions.above = directionY == 1;     // Actualiza el estado de colisión
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}
