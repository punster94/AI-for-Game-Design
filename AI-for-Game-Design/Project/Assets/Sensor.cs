using UnityEngine;
using System.Collections;

public abstract class Sensor {
    private GameObject owner;
    private string senseTag;

    // a sensor has an owner and a senseTag.
    // Owner: object that has (owns) the sensor
    // senseTag: objects to sense are tagged with this string.
    public Sensor(GameObject o, string st) {
        owner = o;
        senseTag = st;
    }

    //sense should return an arraylist of sensed items.
    public abstract ArrayList sense();

    //drawToolTip is for updating and displaying debug GUI elements.
    public abstract void drawTooltip();

    //toString takes in a list of sensedObjects that have been sensed.
    //Then, the object takes these and converts itself to a string.
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

    //Finds all active GameObjects tagged with the senseTag.
    public GameObject[] sensableObjects() {
        return GameObject.FindGameObjectsWithTag(senseTag);
    }
}
