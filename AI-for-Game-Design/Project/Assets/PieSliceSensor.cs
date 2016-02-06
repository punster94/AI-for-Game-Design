using UnityEngine;
using System.Collections;
using System;

public class PieSliceSensor : Sensor {
    private float radius;
    private float relativeDirection;
    private readonly float inverseCosCutoff;
    private readonly float sweepRadiusRadians;
    private readonly int drawAbility = 25;
    private Level activationLevel;

    private LineRenderer lineRenderer;
    
    public Level ActivationLevel
    {
        get
        {
            return activationLevel;
        }
    }

    public enum Level { Zero = 0, Low = 1, Med = 2, High = 3 };
    GameObject owner;
    GameObject lineDrawer;

    //constructor for pieslice. 
    //direction = direction pie slice sensor will face (degrees)
    //sweep = amount of sensor sweep, to the left or right.
    //example: sweep = 90 means the sensor will sweep one quarter circle, 45 degrees left, 45 right.
    public PieSliceSensor(GameObject owner, string sensorTag, float radius, float direction, float sweep) : base(owner, sensorTag)
    {
        relativeDirection = ClampDirection(direction) * Mathf.Deg2Rad;

        //cos double-counts sweep, so we have to reduce it by half.
        sweep /= 2.0f;

        //our cutoff angle for determining objects in sweep or not.
        inverseCosCutoff = Mathf.Cos(Mathf.Abs(sweep) * Mathf.Deg2Rad);

        //our sweep stored in radians instead of degrees.
        sweepRadiusRadians = sweep * Mathf.Deg2Rad;

        //sensor radius.
        this.radius = radius;

        //our object's owner.
        this.owner = owner;

        //initialize the line renderer and line renderer holder object
        InitLineRenderer();

        activationLevel = Level.Zero;
    }

    //initializes the line renderer and holder object
    private void InitLineRenderer()
    {
        //since objects can only have one line renderer, we have to add an object to hold a new line renderer.
        lineDrawer = new GameObject();
        
        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.SetVertexCount(drawAbility + 1);
        lineRenderer.SetWidth(0.25f, 0.25f);
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetColors(Color.white, Color.white);
        lineRenderer.material.color = Color.white;
    }

    //draws the pie using lines.
    public override void drawTooltip()
    {
        //first get the owner position, so we can do local space.
        Vector3 pos = owner.transform.position;
        lineRenderer.SetPosition(0, pos);

        //this controls where our initial direction goes, and how many radian increments between line segements.
        float startDirection = GetLocalDirectionRadians() - sweepRadiusRadians;
        float incrementDirection = sweepRadiusRadians * 2.0f / (drawAbility - 2);

        //draw the circle portion of the arc.
        for (int i = 1; i < drawAbility; i++)
        {
            float circleDir = startDirection + incrementDirection * (i - 1);
            Vector3 circlePos = pos + new Vector3(Mathf.Cos(circleDir) * radius,
                                                  Mathf.Sin(circleDir) * radius);
            lineRenderer.SetPosition(i, circlePos);
        }
        //draw final line for ending.
        lineRenderer.SetPosition(drawAbility, pos);

        //set the color according to activation level.
        if (activationLevel <= Level.Low)
        {
            lineRenderer.material.color = Color.white;
            lineRenderer.SetColors(Color.white, Color.white);
        }
        else if (activationLevel == Level.Med)
        {
            lineRenderer.material.color = Color.blue;
            lineRenderer.SetColors(Color.blue, Color.blue);
        }
        else
        {
            lineRenderer.material.color = Color.red;
            lineRenderer.SetColors(Color.red, Color.red);
        }

        return;
    }

    //used to determine the local adjusted starting direction, in radians. May be > 2 pi.
    private float GetLocalDirectionRadians()
    {
        float parentDirection = owner.transform.eulerAngles.z * Mathf.Deg2Rad;

        // The unity direction is 90 degrees off, so we also add 0.5 pi to the heading.
        return parentDirection + 0.5f * Mathf.PI + relativeDirection;
    }

    //returns list of objects, and resets the current activation level.
    public override ArrayList sense()
    {
        GameObject[] potentialSensedObjects = base.sensableObjects();
        ArrayList sensedObjects = new ArrayList();
        Vector3 parentPos = owner.transform.position;

        float newDirection = GetLocalDirectionRadians();

        //gives a normalized parent heading vector, plus our relative sensor heading.
        Vector3 parentHeading = new Vector3(Mathf.Cos(newDirection), Mathf.Sin(newDirection));

        //the potential objects are those tagged senseable.
        foreach (GameObject sensedObject in potentialSensedObjects)
        {
            Vector3 sensedPos = sensedObject.transform.position;

            //check for distance first, if within the "radius", it does the angle check.
            if (Vector3.Distance(sensedPos, parentPos) <= radius)
            {
                //gives us a normalized heading towards the other object.
                Vector3 relativeHeading = (sensedPos - parentPos).normalized;
                //allows us to calculate the cos of the angle, according to the formula.
                float dotProduct = Vector3.Dot(parentHeading, relativeHeading);

                //rather than doing inverse cos and comparing against the angle, we compare against
                // the pre-computed inverse cos cutoff value.
                if (dotProduct >= inverseCosCutoff)
                    sensedObjects.Add(new AdjacentAgent(sensedObject, parentHeading, sensedPos, parentPos));
            }
        }

        if (sensedObjects.Count == 0)
            activationLevel = Level.Zero;
        else if (sensedObjects.Count <= (int)Level.Low)
            activationLevel = Level.Low;
        else if (sensedObjects.Count <= (int)Level.Med)
            activationLevel = Level.Med;
        else
            activationLevel = Level.High;

        return sensedObjects;
    }

    //clamps a direction to be between 0 and 360 degrees
    private float ClampDirection(float direction)
    {
        if (direction >= 0)
            return direction % 360;
        //if direction < 0, then shift as needed back into range
        int numTimes = (int)(-360 / direction) + 1;
        direction += 360 * numTimes;
        return direction % 360;
    }
    
    private String VectorPrec(Vector3 v)
    {
        return "(" + v.x + "," + v.y + "," + v.z + ")";
    }

    public override string ToString()
    {
        return "PieSlice sensor @ " + relativeDirection * Mathf.Rad2Deg + ", activation level = " + ActivationLevel + ".";
    }
}
