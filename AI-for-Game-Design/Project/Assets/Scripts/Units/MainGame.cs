using UnityEngine;
using System.Collections;
using Graph;
using System.Collections.Generic;

public class MainGame : MonoBehaviour {
	// List of Unit objects
	List<Unit> enemyUnits, allyUnits;
	// Bucket objects for adding game objects to
	GameObject enemyUnitObjects, allyUnitObjects;

    GameObject parent;

	// Use this for initialization
	void Start () {
		enemyUnits = new List<Unit>();
		allyUnits = new List<Unit>();

        parent = transform.gameObject;
        PathFinder pathFinder = parent.GetComponent<PathFinder>();

        //TODO: Integrate PCG here
        pathFinder.initializeGraph(new Vector2(-17, -13), new Vector2(17, 14), 1.0f, 0.75f);
        KeyValuePair<List<Unit>, List<Unit>> spawnPoints = pathFinder.getSpawnPoints();

        enemyUnitObjects = GameObject.Find("Enemy Units");
        allyUnitObjects = GameObject.Find("Ally Units");
        enemyUnitObjects.transform.parent = parent.transform;
        allyUnitObjects.transform.parent = parent.transform;

        enemyUnits.Add(new LongArmUnit(enemyUnitObjects, "Enemy LongArm", 12, 12, true));
		allyUnits.Add(new LongArmUnit(allyUnitObjects, "Ally LongArm", 13, 10, false));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
