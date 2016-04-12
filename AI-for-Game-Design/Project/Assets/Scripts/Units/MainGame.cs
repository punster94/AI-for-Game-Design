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

	Unit currentlySelectedUnit = null;
	bool unitSelected = false;

	// Use this for initialization
	void Start() {
		enemyUnits = new List<Unit>();
		allyUnits = new List<Unit>();

        parent = gameObject;

		enemyUnitObjects = GameObject.Find("Enemy Units");
		allyUnitObjects = GameObject.Find("Ally Units");
		enemyUnitObjects.transform.parent = parent.transform;
		allyUnitObjects.transform.parent = parent.transform;

        pathManager = new GraphManager(parent);
        pathFinder = pathManager.getGraph();

        //TODO: Integrate PCG here
        //OR generate more spawns than needed and randomly select from generated spawn points.
        KeyValuePair<List<Node>, List<Node>> spawnPoints = pathFinder.getSpawnPoints(5);

        for (int i = 0; i < spawnPoints.Key.Count; i++) {
            Node spawnEnemy = spawnPoints.Key[i];
            Node spawnAlly = spawnPoints.Value[i];

			addLongArmUnit(enemyUnits, enemyUnitObjects, "Enemy LongArm", spawnEnemy, true);
			addLongArmUnit(allyUnits, allyUnitObjects, "Ally LongArm", spawnAlly, false);
        }
	}

	void addLongArmUnit(List<Unit> units, GameObject bucket, string name, Node node, bool enemy) {
		Unit newUnit = new LongArmUnit(bucket, name, node, enemy);
		units.Add(newUnit);
		node.Occupier = newUnit;
		newUnit.spriteObject.AddComponent<UnitBehavior>().setUnit(newUnit);
	}
	
	// Update is called once per frame
	void Update() {
		if(Input.GetMouseButtonDown((int)MouseButton.left)) {
			Vector2 position = getMousePos();

			if(clickInGraph(position)) {
				Node clickedNode = pathFinder.closestMostValidNode(position);

				if(unitSelected) {
					currentlySelectedUnit.deselect();
					unitSelected = false;
				}
				

				if(clickedNode.Occupied) {
					currentlySelectedUnit = clickedNode.Occupier;
					currentlySelectedUnit.select();
					unitSelected = true;
					pathFinder.displayRangeOfUnit(currentlySelectedUnit, position);
				}
			}
		}
	}

	private bool clickInGraph(Vector2 position) {
		// TODO: Base these comparisons on the values the MainGame object will specify as the bounding box of the graph
		//if(position.x > && position.x <  && position.y > && position.y < )
			return true;
		//return false;
	}

	private Vector2 getMousePos() {
		return Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
}
