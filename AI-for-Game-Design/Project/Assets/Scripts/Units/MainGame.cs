using UnityEngine;
using System.Collections;
using Graph;
using System.Collections.Generic;

public enum MouseButton { left = 0, right = 1, middle = 2 }

public class MainGame : MonoBehaviour {
	// List of Unit objects
	List<Unit> enemyUnits, allyUnits;
	// Bucket objects for adding game objects to
	GameObject enemyUnitObjects, allyUnitObjects;

    GameObject parent;

	PathFinder pathFinder;
	GraphManager pathManager;
    TurnManager turnManager;

	// Use this for initialization
	void Awake() {
		enemyUnits = new List<Unit>();
		allyUnits = new List<Unit>();

        parent = gameObject;

		enemyUnitObjects = GameObject.Find("Enemy Units");
		allyUnitObjects = GameObject.Find("Ally Units");
		enemyUnitObjects.transform.parent = parent.transform;
		allyUnitObjects.transform.parent = parent.transform;

        // pathManager = new GraphManager(parent);
		PCG generator = new PCG();
        pathManager = new GraphManager(parent, generator.generateMap());

        pathFinder = pathManager.getGraph();

        //TODO: Integrate PCG here
        //OR generate more spawns than needed and randomly select from generated spawn points.
        Vector2 topright = pathFinder.getTopRightBound();
        Vector2 botleft = pathFinder.getBottomLeftBound();
        Vector2 mid = topright - botleft;
        mid /= 8;
        KeyValuePair<List<Node>, List<Node>> spawnPoints = pathFinder.getSpawnPoints(topright - mid, botleft + mid, 6);

        for (int i = 0; i < spawnPoints.Key.Count; i++) {
            Node spawnEnemy = spawnPoints.Key[i];
            Node spawnAlly = spawnPoints.Value[i];

            float spawnType = Random.value;
            
            if (spawnType < 0.3)
            {
                addLongArmUnit(enemyUnits, enemyUnitObjects, "Enemy LongArm", spawnEnemy, true);
                addLongArmUnit(allyUnits, allyUnitObjects, "Ally LongArm", spawnAlly, false);
            }
            else if (spawnType < 0.55)
            {
                addBigGuyUnit(enemyUnits, enemyUnitObjects, "Enemy BigGuy", spawnEnemy, true);
                addBigGuyUnit(allyUnits, allyUnitObjects, "Ally BigGuy", spawnAlly, false);
            }
            else if (spawnType < 0.85)
            {
                addLongRangeUnit(enemyUnits, enemyUnitObjects, "Enemy LongRange", spawnEnemy, true);
                addLongRangeUnit(allyUnits, allyUnitObjects, "Ally LongRange", spawnAlly, false);
            }
            else
            {
                addRunnerUnit(enemyUnits, enemyUnitObjects, "Enemy Runner", spawnEnemy, true);
                addRunnerUnit(allyUnits, allyUnitObjects, "Ally Runner", spawnAlly, false);
            }
        }

        enemyUnits.Reverse();
        allyUnits.Reverse();

        turnManager = new TurnManager(pathFinder, allyUnits, enemyUnits);
	}

	void addLongArmUnit(List<Unit> units, GameObject bucket, string name, Node node, bool enemy)
    {
		Unit newUnit = new LongArmUnit(bucket, name, node, enemy);
		units.Add(newUnit);
		node.Occupier = newUnit;
		newUnit.spriteObject.AddComponent<UnitBehavior>().setUnit(newUnit);
	}
    
    void addBigGuyUnit(List<Unit> units, GameObject bucket, string name, Node node, bool enemy)
    {
        Unit newUnit = new BigGuyUnit(bucket, name, node, enemy);
        units.Add(newUnit);
        node.Occupier = newUnit;
        newUnit.spriteObject.AddComponent<UnitBehavior>().setUnit(newUnit);
    }

    void addRunnerUnit(List<Unit> units, GameObject bucket, string name, Node node, bool enemy)
    {
        Unit newUnit = new RunnerUnit(bucket, name, node, enemy);
        units.Add(newUnit);
        node.Occupier = newUnit;
        newUnit.spriteObject.AddComponent<UnitBehavior>().setUnit(newUnit);
    }

    void addLongRangeUnit(List<Unit> units, GameObject bucket, string name, Node node, bool enemy)
    {
        Unit newUnit = new LongRangeUnit(bucket, name, node, enemy);
        units.Add(newUnit);
        node.Occupier = newUnit;
        newUnit.spriteObject.AddComponent<UnitBehavior>().setUnit(newUnit);
    }
    // Update is called once per frame
    void Update()
    {
        turnManager.Update();
    }
}
