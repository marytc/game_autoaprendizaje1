using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Controlador para plataformas que se mueven
public class PlatformController : RaycastController
{
    // Máscara de capas para los pasajeros que interactúan con la plataforma
    public LayerMask passengerMask;

    // Puntos locales que definen el camino de la plataforma
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints; // Puntos globales calculados a partir de los locales

    // Velocidad de la plataforma
    public float speed;
    public bool cyclic; // Indica si la plataforma debe moverse en un ciclo
    public float waitTime; // Tiempo de espera al llegar a un punto
    [Range(0, 2)]
    public float easeAmount; // Cantidad de suavizado en el movimiento

    // Índices y variables para el movimiento de la plataforma
    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    // Lista para almacenar el movimiento de los pasajeros
    List<PassengerMovement> passengerMovement;
    // Diccionario para almacenar los pasajeros y sus controladores
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    // Método que se llama al inicio
    public override void Start()
    {
        base.Start(); // Llama al método Start de la clase base

        // Inicializa los puntos globales a partir de los locales
        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position; // Convierte a coordenadas globales
        }
    }

    // Método que se llama en cada frame
    void Update()
    {
        UpdateRaycastOrigins(); // Actualiza los orígenes de los raycasts

        Vector3 velocity = CalculatePlatformMovement(); // Calcula el movimiento de la plataforma

        CalculatePassengerMovement(velocity); // Calcula el movimiento de los pasajeros

        MovePassengers(true); // Mueve a los pasajeros antes de mover la plataforma
        transform.Translate(velocity); // Mueve la plataforma
        MovePassengers(false); // Mueve a los pasajeros después de mover la plataforma
    }

    // Método para suavizar el movimiento
    float Ease(float x)
    {
        float a = easeAmount + 1; // Ajusta el parámetro de suavizado
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a)); // Calcula el valor suavizado
    }

    // Calcula el movimiento de la plataforma
    Vector3 CalculatePlatformMovement()
    {
        // Si no es el momento de mover la plataforma, retorna cero
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        // Calcula los índices de los puntos de la plataforma
        fromWaypointIndex %= globalWaypoints.Length; // Asegura que el índice esté dentro de los límites
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length; // Índice del siguiente punto
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]); // Distancia entre puntos
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints; // Calcula el porcentaje de movimiento
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints); // Asegura que esté entre 0 y 1
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints); // Aplica suavizado

        // Calcula la nueva posición de la plataforma
        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        // Si se ha llegado al siguiente punto
        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0; // Reinicia el porcentaje
            fromWaypointIndex++; // Avanza al siguiente índice

            // Si no es cíclico, verifica si se ha llegado al final
            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0; // Reinicia el índice
                    System.Array.Reverse(globalWaypoints); // Invierte el orden de los puntos
                }
            }
            nextMoveTime = Time.time + waitTime; // Establece el tiempo de espera
        }

        return newPos - transform.position; // Retorna el movimiento de la plataforma
    }

    // Mueve a los pasajeros en función de su estado
    void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            // Si el pasajero no está en el diccionario, lo agrega
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }

            // Mueve al pasajero si corresponde
            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    // Calcula el movimiento de los pasajeros en función de la velocidad de la plataforma
    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>(); // Conjunto para evitar duplicados
        passengerMovement = new List<PassengerMovement>(); // Lista para almacenar el movimiento de los pasajeros

        float directionX = Mathf.Sign(velocity.x); // Dirección en X
        float directionY = Mathf.Sign(velocity.y); // Dirección en Y

        // Plataforma en movimiento vertical
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth; // Longitud del rayo

            for (int i = 0; i < verticalRayCount; i++)
            {
                // Origen del rayo
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i); // Ajusta el origen del rayo
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask); // Lanza el rayo

                if (hit) // Si hay un impacto
                {
                    if (!movedPassengers.Contains(hit.transform)) // Si el pasajero no ha sido movido
                    {
                        movedPassengers.Add(hit.transform); // Agrega al conjunto
                        float pushX = (directionY == 1) ? velocity.x : 0; // Calcula el empuje en X
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY; // Calcula el empuje en Y

                        // Agrega el movimiento del pasajero
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        // Plataforma en movimiento horizontal
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth; // Longitud del rayo

            for (int i = 0; i < horizontalRayCount; i++)
            {
                // Origen del rayo
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i); // Ajusta el origen del rayo
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask); // Lanza el rayo

                if (hit) // Si hay un impacto
                {
                    if (!movedPassengers.Contains(hit.transform)) // Si el pasajero no ha sido movido
                    {
                        movedPassengers.Add(hit.transform); // Agrega al conjunto
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX; // Calcula el empuje en X
                        float pushY = -skinWidth; // Empuje en Y

                        // Agrega el movimiento del pasajero
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Pasajero en la parte superior de una plataforma en movimiento horizontal o hacia abajo
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2; // Longitud del rayo

            for (int i = 0; i < verticalRayCount; i++)
            {
                // Origen del rayo
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask); // Lanza el rayo

                if (hit) // Si hay un impacto
                {
                    if (!movedPassengers.Contains(hit.transform)) // Si el pasajero no ha sido movido
                    {
                        movedPassengers.Add(hit.transform); // Agrega al conjunto
                        float pushX = velocity.x; // Empuje en X
                        float pushY = velocity.y; // Empuje en Y

                        // Agrega el movimiento del pasajero
                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    // Estructura para almacenar el movimiento de los pasajeros
    struct PassengerMovement
    {
        public Transform transform; // Transform del pasajero
        public Vector3 velocity; // Velocidad del pasajero
        public bool standingOnPlatform; // Indica si está de pie sobre la plataforma
        public bool moveBeforePlatform; // Indica si se mueve antes de la plataforma

        // Constructor para inicializar la estructura
        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    // Método para dibujar gizmos en el editor
    void OnDrawGizmos()
    {
        if (localWaypoints != null) // Si hay puntos locales definidos
        {
            Gizmos.color = Color.red; // Establece el color de los gizmos
            float size = .3f; // Tamaño de los gizmos

            // Dibuja líneas para cada punto local
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position; // Posición global
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size); // Línea vertical
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size); // Línea horizontal
            }
        }
    }
}
