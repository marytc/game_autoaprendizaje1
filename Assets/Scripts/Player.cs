using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    float gravity;
    float jumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;        // Control raycast y colisiones

    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2); // Calcula gravedad para que el jugador pueda alcanzar una altura máxima al saltar
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;         // La velocidad de salto se determina en función de la gravedad
        print("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);  // Imprime en la consola los valores calculados de gravedad y velocidad de salto para depuración
    }

    void Update()
    {
        // Resetea la velocidad vertical si hay colisión arriba o abajo
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Maneja el salto
        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = jumpVelocity;      // Asigna la velocidad de salto
        }

        float targetVelocityX = input.x * moveSpeed;    // Determina la velocidad deseada en el eje X según la entrada del jugador
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);  // Suaviza la transición entre la velocidad actual y la velocidad objetivo

        velocity.y += gravity * Time.deltaTime;     // Aplica la gravedad al jugador
        controller.Move(velocity * Time.deltaTime); // Actualiza la posición del jugador según la velocidad calculada

    }
}
