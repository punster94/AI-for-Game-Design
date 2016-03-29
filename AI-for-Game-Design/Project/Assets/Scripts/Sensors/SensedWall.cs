using UnityEngine;
using System.Collections;

public class SensedWall : SensedObject {
    private string name;
    private float distance;
    private Vector3 intersection;

    public SensedWall(string wallName, float distance, Vector3 intersection)
    {
        this.name = wallName;
        this.distance = distance;
        this.intersection = intersection;
    }

    public override string toString()
    {
        return name + " " + distance + " units away";
    }
}
