using UnityEngine;
using System.Collections;

public class AdjacentAgent : SensedObject {
    public string id;
    public Vector3 position;
    public Vector3 relativeHeading; 

    public AdjacentAgent(GameObject go, Vector3 p, Vector3 ownerPosition) {
        position = p;
        id = go.name;
        relativeHeading = p - ownerPosition;
    }

    public override string toString(){
        return "Id: " + id + " position: " + position + " " + relativeHeading + "\n";
    }
}
