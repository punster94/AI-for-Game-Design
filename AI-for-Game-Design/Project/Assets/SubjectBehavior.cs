using UnityEngine;
using System.Collections;

public class SubjectBehavior : MonoBehaviour {

    static float maxSpeed = 12f;
    static float accelerationRate = 10f;
    static float frictionRate = 0.95f;
    static float backRate = 6f;
    static float turnRate = 2f;

    Vector3 currentSpeed;

    // Use this for initialization
    void Start() {
        currentSpeed.x = currentSpeed.y = currentSpeed.z = 0;
    }

    // Update is called once per frame
    void Update() {
        // Transform the angle of the subject based on the A and D keys
        turnSubject();

        // Don't calculate a new position if the stop key X is pressed
        if (Input.GetKey(KeyCode.X)) {
            Start();
            return;
        }

        // Adjust the current speed by adding acceleration or braking (thats how you spell it I guess) forces based on the W and S keys
        repositionSubject();
    }

    void turnSubject() {
        float direction = 0f;

        if (Input.GetKey(KeyCode.A))
            direction = 1f;
        else if (Input.GetKey(KeyCode.D))
            direction = -1f;

        transform.Rotate(Vector3.forward, direction * turnRate);
    }

    void repositionSubject() {
        if (Input.GetKey(KeyCode.W))
            currentSpeed += accelerationRate* transform.up * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            currentSpeed -= backRate* transform.up * Time.deltaTime;
        else
            currentSpeed *= frictionRate;

        // Truncate the speed vector if it is too fast
        if (currentSpeed.magnitude > maxSpeed)
            currentSpeed = maxSpeed* Vector3.Normalize(currentSpeed);

        // Transform the x, y (and z) position of the subject
        transform.position += currentSpeed* Time.deltaTime;
    }
}
