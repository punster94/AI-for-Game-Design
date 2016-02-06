using UnityEngine;
using System.Collections;

public class AdjacentAgent : SensedObject {
    public string id;
    public double distance;
    public double relativeHeading; 

    public AdjacentAgent(Vector3 ownerHeading, Vector3 p, Vector3 ownerPosition) {
        distance = Vector3.Distance(p, ownerPosition);
        relativeHeading = Vector3.Angle(ownerHeading, p - ownerPosition);
    }

    public override string toString(){
        return "(" + distance + "," + relativeHeading + ")";
    }
}
