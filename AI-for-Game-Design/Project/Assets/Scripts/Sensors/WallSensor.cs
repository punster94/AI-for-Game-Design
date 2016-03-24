using UnityEngine;
using System.Collections;
using System;

public class WallSensor : Sensor
{
    private static string LAYER_FILTER_STRING = "Walls"; // String (name) of the Unity2D layer that all walls are on
    private string tagString; // Required by base class, but not really used
    private float maxDistance;
    private float relativeDirectionDegrees;
    private GameObject owner;
    private GameObject lineDrawer;
    private LineRenderer lineRenderer;

    public WallSensor(GameObject owner, string tagString, float relativeDirectionDegrees, float maxDistance) : base(owner, tagString)
    {
        this.tagString = tagString;
        this.owner = owner;
        // Our sprite is set up so that "right" is actually 0 degrees and "forward" is 90
        this.relativeDirectionDegrees = (relativeDirectionDegrees + 90.0f) % 360.0f;
        this.maxDistance = maxDistance;

        InitLineRenderer();
    }

    private void InitLineRenderer()
    {
        // Create a new object to hold the line renderer
        lineDrawer = new GameObject();

        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetWidth(0.1f, 0.1f);
        lineRenderer.useWorldSpace = true;
        lineRenderer.material.color = Color.magenta; // Magenta, because why not?
    }

    // Find the endpoint of this sensor in world space
    private Vector3 getEndpoint()
    {
        Vector3 endPosition = ownerPosition() + getAbsoluteDirectionVector() * this.maxDistance;

        return endPosition;
    }

    public override void drawTooltip()
    {
        // Draw a simple line =)
        lineRenderer.SetPosition(0, ownerPosition());
        lineRenderer.SetPosition(1, getEndpoint());
    }

    public override ArrayList sense()
    {
        // Super-class requires returning ArrayList, even though this particular sensor will only "find" the nearest wall (or none)
        ArrayList nearestWall = new ArrayList();

        Vector2 origin = new Vector2(ownerPosition().x, ownerPosition().y);

        // Raycast, filtering out everything not on the "walls" layer. Only the first object hit is returned from Raycast.
        RaycastHit2D hitWall = Physics2D.Raycast(origin, getAbsoluteDirectionVector(), this.maxDistance, LayerMask.GetMask(LAYER_FILTER_STRING));
        if(hitWall.collider != null)
        {
            nearestWall.Add(new SensedWall(hitWall.collider.gameObject.name, hitWall.distance, new Vector3(hitWall.point.x, hitWall.point.y)));
        }

        return nearestWall;
    }

    public override string toString(ArrayList sensedObjects)
    {
        String output = "WallSensor on " + ownerName() + " @ " + (this.relativeDirectionDegrees - 90.0f);

        // If we didn't hit anything
        if (sensedObjects.Count < 1)
            output += " detected nothing.";
        else
        {
            // We sensed something, output it to the console
            SensedObject obj = (SensedObject)sensedObjects[0];
            output += " detected wall " + obj.toString() + ".";
        }

        return output;
    }

    // Get the direction vector of this sensor in world-space terms
    public Vector3 getAbsoluteDirectionVector()
    {
        // Find the owner's heading
        float ownerDirectionDegrees = owner.transform.eulerAngles.z;

        // Add the owner's heading to the relative heading, keeping it between 0.0f and 360.0f
        float absoluteHeading = (ownerDirectionDegrees + relativeDirectionDegrees) % 360.0f;

        float x = Mathf.Cos(absoluteHeading * Mathf.Deg2Rad);
        float y = Mathf.Sin(absoluteHeading * Mathf.Deg2Rad);

        return new Vector3(x, y);
    }
}
