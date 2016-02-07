﻿using UnityEngine;
using System.Collections;
using System;

public class AdjacentAgentSensor : Sensor {
    private float radius;
    private static string tooltipTag = "Adjacent Agent Sensor Tooltip";
    private GameObject tooltip;

    public AdjacentAgentSensor(GameObject o, string st, float r) : base(o, st) {
        radius = r;
        tooltip = GameObject.Find(tooltipTag);
        tooltip.transform.localScale = new Vector3(radius * 2, radius * 2);
    }

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

    public override void drawTooltip() {
        tooltip.transform.position = ownerPosition();
    }

    public override string toString(ArrayList sensedObjects) {
        string logMessage =  "Adjacent Agent Sensor on " + ownerName() + ": ";
        foreach(SensedObject sensedObj in sensedObjects)
            logMessage += (sensedObj.toString() + ", ");
        return logMessage;
    }
}
