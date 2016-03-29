using UnityEngine;
using System.Collections;
using System;

public class AdjacentAgentSensor : Sensor {
    private float radius;
    GameObject imgObj;
    SpriteRenderer drawer;

    public AdjacentAgentSensor(GameObject o, string st, float r) : base(o, st) {
        radius = r;
        
        Sprite nodeImg = Resources.Load<Sprite>("Static Agent");
        imgObj = new GameObject("AdjacentAgent Sensor");
        drawer = imgObj.AddComponent<SpriteRenderer>();
        drawer.sprite = nodeImg;
        drawer.color = new Color(1, 0, 0, 0.25f);
        drawer.transform.localScale = new Vector3(radius * 2, radius * 2);
    }

    //senses nearby objects in range, returning the adjacent agents in an ArrayList
    public override ArrayList sense() {
        ArrayList potentiallySensedObjects = new ArrayList(sensableObjects()), adjacentAgents = new ArrayList();
        Vector3 op = ownerPosition();
        Vector3 oh = ownerHeading();

        foreach(GameObject go in potentiallySensedObjects) {
            Vector3 position = go.transform.position;

            if (Vector3.Distance(position, op) <= radius)
                adjacentAgents.Add(new AdjacentAgent(go, oh, position, op));
        }

        return adjacentAgents;
    }

    //changes the position of the circle on the screen
    public override void drawTooltip()
    {
        if (Draw)
        {
            drawer.enabled = true;
            imgObj.transform.position = ownerPosition();
        }
        else
            drawer.enabled = false;
    }

    //takes in a list of sensedObjects that were sensed in a sense call and converts the object to a string.
    public override string toString(ArrayList sensedObjects) {
        string logMessage =  "Adjacent Agent Sensor on " + ownerName() + ": ";
        foreach(SensedObject sensedObj in sensedObjects)
            logMessage += (sensedObj.toString() + ", ");
        return logMessage;
    }
}
