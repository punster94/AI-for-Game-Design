using UnityEngine;
using System.Collections;

public class SubjectBehavior : MonoBehaviour {

    GameObject self;

    Vector3 currentSpeed;

    // Physics properties
    static float maxSpeed = 12f;
    static float accelerationRate = 10f;
    static float frictionRate = 0.95f;
    static float backRate = 6f;
    static float turnRate = 4f;

    // Sensor properties
    int frame;
    static int framesPerSense = 150;
    static float aasRadius = 5f;
    static string selfTag = "Subject";
    static string sensableTag = "Sensable Agent";
    static string wallTag = "Wall";

    // Sensors available to the subject
    ArrayList sensors = new ArrayList();

    // Use this for initialization
    void Start() {
        // Initialize reference to self and sensor objects
        self = GameObject.Find(selfTag);
        sensors.Add(new AdjacentAgentSensor(self, sensableTag, aasRadius));
        sensors.Add(new PieSliceSensor(self, sensableTag, aasRadius * 1.3f, 0, 90));
        sensors.Add(new PieSliceSensor(self, sensableTag, aasRadius, 90, 90));
        sensors.Add(new PieSliceSensor(self, sensableTag, aasRadius * 0.5f, 180, 90));
        sensors.Add(new PieSliceSensor(self, sensableTag, aasRadius, 270, 90));
        sensors.Add(new WallSensor(self, wallTag, 10.0f, 3));
        sensors.Add(new WallSensor(self, wallTag, -10.0f, 3));
        sensors.Add(new WallSensor(self, wallTag, 30.0f, 2));
        sensors.Add(new WallSensor(self, wallTag, -30.0f, 2));
        sensors.Add(new WallSensor(self, wallTag, 180.0f, 1));
        frame = 0;

        // Initialize speed at zero
        initializeSpeed();
    }

    void initializeSpeed() {
        currentSpeed.x = currentSpeed.y = currentSpeed.z = 0;
    }

    // Update is called once per frame
    void Update() {
        // Transform the angle of the subject based on the A and D keys
        turnSubject();

        // Don't calculate a new position if the stop key X is pressed
        if (Input.GetKey(KeyCode.X)) {
            initializeSpeed();
            return;
        }

        // Adjust the current speed by adding acceleration or braking (thats how you spell it I guess) forces based on the W and S keys
        repositionSubject();

        // Sense the world
        if (frame++ == framesPerSense) {
        //note that assignment requires updating every tick.
            sense();
            // Keeps the frame value low, but can be maintained with modulus if frame count is needed for something else
           frame = 0;
        }

        // Print the tooltip for each sensor owned by the subject
        foreach(Sensor s in sensors) {
            s.drawTooltip();
        }
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
    
    void sense() {
        Debug.Log("Subject at location: (" + self.transform.position.x + ", " + self.transform.position.y + ") and heading " + Vector3.Angle(self.transform.up, new Vector3(0f, 1f)));
        foreach(Sensor s in sensors) {
            ArrayList sensedObjects = s.sense();
            Debug.Log(s.toString(sensedObjects));
        }
    }
}
