using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Graph;

public class SubjectBehavior : MonoBehaviour {

    //New controls
    const bool oldControls = false;
    const float doneDist = 0.07f;
    const float diffDist = 1.2f;
    float origDist = float.PositiveInfinity;
    float minDist = float.PositiveInfinity;
    Physics2D physController;
    Queue<Vector2> targets = new Queue<Vector2>();
    //Should be loaded into a constants file
    private static int LAYER_FILTER_MASK = LayerMask.GetMask("Walls");

    PathFinder pathfinder;

    public enum MouseButton { left = 0, right = 1, middle = 2 }
    

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
    List<Sensor> sensors = new List<Sensor>();

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

        pathfinder = PathManager.getDenseGraph();

        // Initialize speed at zero
        initializeSpeed();
    }

    void initializeSpeed() {
        currentSpeed.x = currentSpeed.y = currentSpeed.z = 0;
    }

    // Update is called once per frame
    void Update() {

        if (!oldControls)
        {
            // Transform the angle of the subject based on the A and D keys
            turnSubject();

            // Don't calculate a new position if the stop key X is pressed
            if (Input.GetKey(KeyCode.X))
            {
                initializeSpeed();
                return;
            }

            // Adjust the current speed by adding acceleration or braking (thats how you spell it I guess) forces based on the W and S keys
            repositionSubject();
        }

        //Add else to place this on. Currently runs both options.
        {
            if (Input.GetKeyDown("r"))
            {
                pathfinder.onlineRecreateGraph();
            }

            if (Input.GetMouseButtonDown((int)MouseButton.left))
                seek(getMousePos());
            else if (Input.GetMouseButton((int)MouseButton.middle))
                targets.Enqueue(getMousePos());
            else if (Input.GetMouseButtonDown((int)MouseButton.right))
            {
                //AStar pathfinding
                targets.Clear();
                Queue<Node> path = new Queue<Node>();
                pathfinder.AStar(path, transform.position, getMousePos());
                foreach (Node n in path)
                    targets.Enqueue(new Vector2(n.getPos().x, n.getPos().y));
            }

            updateSeek();
        }
        
        // Sense the world
        if (frame++ == framesPerSense)
        {
            //note that assignment requires updating every tick.
            sense();
            // Keeps the frame value low, but can be maintained with modulus if frame count is needed for something else
            frame = 0;
        }

        // Print the tooltip for each sensor owned by the subject
        foreach (Sensor s in sensors)
        {
            s.drawTooltip();
        }

        // Toggles graph visiblity
        if (Input.GetKeyDown("p"))
            pathfinder.graphDisplay(!pathfinder.graphIsDisplayed());
    }

    private Vector2 getMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Seeks a Vector2 target location.
    /// </summary>
    /// <param name="target">The target to seek.</param>
    private void seek(Vector2 target)
    {
        transform.Rotate(Vector3.forward,
                        getAbsoluteAngle(transform.up, target - (Vector2)(transform.position)));
        origDist = Vector2.Distance(transform.position, target) * diffDist;
        minDist = float.PositiveInfinity;
        targets.Clear();
        targets.Enqueue(target);
    }

    /// <summary>
    /// Updats the current seeking location.
    /// </summary>
    private void updateSeek()
    {
        // Seek method
        if (targets.Count > 0)
        {
            Vector2 currentTarget = targets.Peek();

            //If we're done with this target, or not getting anywhere, dequeue the next target.
            float newDist = Vector2.Distance(transform.position, currentTarget);
            if (newDist < doneDist || newDist > minDist * diffDist)
            {
                transform.position = currentTarget;
                targets.Dequeue();
                origDist = Vector2.Distance(transform.position, currentTarget) * diffDist;
                minDist = float.PositiveInfinity;
            }
            //Otherwise continue making our way towards the target.
            else
            {
                transform.Rotate(Vector3.forward,
                                getAbsoluteAngle(transform.up, currentTarget - (Vector2)(transform.position)));
                transform.position += transform.up.normalized * Time.deltaTime * doneDist * maxSpeed * 5.0f;
                if (newDist < minDist)
                    minDist = newDist + diffDist;
            }
        }
    }

    /// <summary>
    /// Returns the absolute (0-360) degree angle between two vectors.
    /// </summary>
    /// <returns>The absolute (0-360) degree angle between two vectors.</returns>
    private float getAbsoluteAngle(Vector2 v1, Vector2 v2)
    {
        float angle = Vector2.Angle(v1, v2);

        if (AngleGreaterThan180(v1, v2))
            angle = 360 - angle;

        return angle;
    }

    //we use the cross product to detect the absolute heading.
    private bool AngleGreaterThan180(Vector2 v1, Vector2 v2)
    {
        //see http://stackoverflow.com/questions/7785601/detecting-if-angle-is-more-than-180-degrees
        // for calculation of angle > 180 degrees.

        return (v1.x * v2.y - v2.x * v1.y) < 0;
    }

    void turnSubject() {
        float direction = 0f;

        if (Input.GetKey(KeyCode.A))
            direction = 1f;
        else if (Input.GetKey(KeyCode.D))
            direction = -1f;

        transform.Rotate(Vector3.forward, direction * turnRate);
    }

    //this allows us to update subject position based on current speed and acceleration.
    void repositionSubject() {
        if (Input.GetKey(KeyCode.W))
            currentSpeed += accelerationRate * transform.up * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            currentSpeed -= backRate * transform.up * Time.deltaTime;
        else
            currentSpeed *= frictionRate;

        // Truncate the speed vector if it is too fast
        if (currentSpeed.magnitude > maxSpeed)
            currentSpeed = maxSpeed * Vector3.Normalize(currentSpeed);

        // Transform the x, y (and z) position of the subject
        transform.position += currentSpeed * Time.deltaTime;
    }
    
    //updates each sensor, and for now debug-prints the data produced.
    void sense() {
        //Debug.Log("Subject at location: (" + self.transform.position.x + ", " + self.transform.position.y + ") and heading " + self.transform.rotation.eulerAngles.z);
        foreach(Sensor s in sensors) {
            //ArrayList sensedObjects = s.sense();
            //Debug.Log(s.toString(sensedObjects));
        }
    }
}
