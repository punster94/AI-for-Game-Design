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
        GraphManager pathManager = new GraphManager(parent);
        PathFinder pathFinder = pathManager.getGraph();

        //TODO: Integrate PCG here
        //OR generate more spawns than needed and randomly select from generated spawn points.
        KeyValuePair<List<Node>, List<Node>> spawnPoints = pathFinder.getSpawnPoints(5);

        enemyUnitObjects = GameObject.Find("Enemy Units");
        allyUnitObjects = GameObject.Find("Ally Units");
        enemyUnitObjects.transform.parent = parent.transform;
        allyUnitObjects.transform.parent = parent.transform;

        for (int i = 0; i < spawnPoints.Key.Count; i++)
        {
            Node spawnEnemy = spawnPoints.Key[i];
            Node spawnAlly = spawnPoints.Value[i];
            enemyUnits.Add(new LongArmUnit(enemyUnitObjects, "Enemy LongArm", spawnEnemy, true));
            allyUnits.Add(new LongArmUnit(allyUnitObjects, "Ally LongArm", spawnAlly, false));
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
