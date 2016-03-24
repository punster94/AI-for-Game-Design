using UnityEngine;
using System.Collections;

public class AdjacentAgent : SensedObject {
    public string id;
    public double distance;
    public double relativeHeading;
    public readonly GameObject agentRef;

    //The AdjacentAgent is a sensed object which has a relative heading, a distance to object, and an id.
    public AdjacentAgent(GameObject agent, Vector3 subjectHeading, Vector3 p, Vector3 subjectPosition) {
        distance = Vector3.Distance(p, subjectPosition);

        Vector3 agentHeading = (p - subjectPosition).normalized;
        relativeHeading = Mathf.Acos(Vector3.Dot(subjectHeading.normalized, (p - subjectPosition).normalized)) * Mathf.Rad2Deg;

        //corrects for relative heading only being between 0-180; fixes it to be from 0-360
        if (AngleGreaterThan180(agentHeading, subjectHeading)) {
            relativeHeading = 360 - relativeHeading;
        }

        id = agent.name;
        agentRef = agent;
    }

    public override string toString() {
        return "(id = " + id + ", distance = " + distance.ToString("F") + ", direction = " + relativeHeading.ToString("F") + ")";
    }

    //we must detect the absolute heading.
    private bool AngleGreaterThan180(Vector3 v1, Vector3 v2)
    {
        //see http://stackoverflow.com/questions/7785601/detecting-if-angle-is-more-than-180-degrees
        // for calculation of angle > 180 degrees.

        return (v1.x * v2.y - v2.x * v1.y) < 0;
    }
}
