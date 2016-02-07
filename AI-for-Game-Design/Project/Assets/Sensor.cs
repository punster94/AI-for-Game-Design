using UnityEngine;
using System.Collections;

public abstract class Sensor {
    private GameObject owner;
    private string senseTag;

    public Sensor(GameObject o, string st) {
        owner = o;
        senseTag = st;
    }

    public abstract ArrayList sense();
    public abstract void drawTooltip();
    public abstract string toString(ArrayList sensedObjects);

    public string ownerName() {
        return owner.name;
    }

    public Vector3 ownerPosition() {
        return owner.transform.position;
    }

    public Vector3 ownerHeading() {
        return owner.transform.up;
    }

    public GameObject[] sensableObjects() {
        return GameObject.FindGameObjectsWithTag(senseTag);
    }
}
