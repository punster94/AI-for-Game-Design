using UnityEngine;
using System.Collections;

public class MainGame : MonoBehaviour {
	// List of Unit objects
	ArrayList enemyUnits, allyUnits;
	// Bucket objects for adding game objects to
	GameObject enemyUnitObjects, allyUnitObjects;

	// Use this for initialization
	void Start () {
		enemyUnits = new ArrayList();
		allyUnits = new ArrayList();
		enemyUnitObjects = GameObject.Find("Enemy Units");
		allyUnitObjects = GameObject.Find("Ally Units");

		enemyUnits.Add(new LongArmUnit(enemyUnitObjects, "Enemy LongArm", 12, 12, true));
		allyUnits.Add(new LongArmUnit(allyUnitObjects, "Ally LongArm", 13, 10, false));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
