using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller2D : RaycastController
{

    public CollisionInfo collisions;

    public override void Start()
    {
        base.Start();
    }

    // Metodo para mover al jugador
    public void Move(Vector3 velocity, bool standingOnPlatform = false)
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

        if (standingOnPlatform)
        {
            collisions.below = true;
        }
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
                if (hit.distance == 0)
                {
                   //continue;
                }
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




    // Estructura para almacenar la información de colisión
    public struct CollisionInfo
    {
        public bool above, below; // Indica si hay colisión arriba o abajo
        public bool left, right;  // Indica si hay colisión a la izquierda o derecha

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}
