﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Graph;

public abstract class Unit {
	// Fields for animation control
	public GameObject spriteObject;
	public int ticks;
	public static int ticksPerAnimation = 100;
	public int animationFrame;

	// Fields for pathing control
	Queue<Vector2> targets = new Queue<Vector2>();
	PathFinder pathfinder;
	const float doneDist = 0.07f;
	const float diffDist = 1.2f;
	static float maxSpeed = 18f;
	float origDist = float.PositiveInfinity;
	float minDist = float.PositiveInfinity;

	// Fields for action control
	private bool currentlySelected = false;
	private bool hasNotMovedThisTurn = true;
    private bool canUndo = true;
	public Transform transform;

    public enum UnitStates
    {
        RunningAway, FindingUnit, Attacking
    }

    public UnitStates CurrentState { get; set; }

    private System.Action callbackFunction = UnitAction.DontCallBack;
    private int storedCostOfMove;
    private Node prevNode;

    private int clay;
	private int currentWater;
	private int maxWater;
	private int bendiness;
	private int hardness;

	private int minAttackRange;
	private int maxAttackRange;

	private Node node;
	private bool enemy;

	// TODO: Make a check to only path to a spot that is in range AND unoccupied
	// TODO: We also want to make sure the user can undo moves before completely committing to them
	public void Update() {
        /*
        // TODO: Remove this logic, replace with only updateSeek();
		if(currentlySelected && hasNotMovedThisTurn) {
			if(Input.GetMouseButtonDown((int)MouseButton.right)) {
				//AStar pathfinding
				targets.Clear();
				Queue<Node> path = new Queue<Node>();
				double cost = getPathFinder().AStar(path, transform.position, getMousePos());
                if (cost > currentWater)
                    return;
				foreach(Node n in path)
					targets.Enqueue(new Vector2 (n.getPos().x, n.getPos().y));
                currentWater -= (int) cost;
				//hasNotMovedThisTurn = false;
			}
		}*/

		updateSeek();
	}

    private PathFinder pathFindRef;
	/// <summary>
	/// treat the pathfinder like a singleton...
	/// </summary>
	/// <returns></returns>
	private PathFinder getPathFinder() {
        if (pathFindRef == null)
		    pathFindRef = transform.parent.parent.GetComponentInChildren<PathFinder>();
        return pathFindRef;
	}

	private Vector2 getMousePos() {
		return Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}

    public void moveUnit(System.Action callBack, Node target) {
        canUndo = true;
        prevNode = getNode();
        storedCostOfMove = 0;
        callbackFunction = callBack;

        //AStar pathfinding
        targets.Clear();
        Queue<Node> path = new Queue<Node>();

        double cost = getPathFinder().AStar(path, getNode(), target);

        //if (cost > currentWater)
        //    return;
        foreach (Node n in path)
            targets.Enqueue(new Vector2(n.getPos().x, n.getPos().y));

        storedCostOfMove = (int)cost;
        currentWater -= storedCostOfMove;
        hasNotMovedThisTurn = false;
    }

    public void resetTurn()
    {
        currentWater = maxWater;
        hasNotMovedThisTurn = true;
        canUndo = false;
        callbackFunction = UnitAction.DontCallBack;
    }

    /// <summary>
    /// Tries to undo an action. If successful, returns true, else false.
    /// </summary>
    /// <returns>If successful, returns true, else false.</returns>
    public bool undoIfPossible() {
        if (canUndo)
        {
            node = prevNode;
            hasNotMovedThisTurn = true;
            currentWater += storedCostOfMove;
            callbackFunction = UnitAction.DontCallBack;
            canUndo = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the current seeking location.
    /// </summary>
    private void updateSeek() {
		// Seek method
        // TODO: fix multiseek units
		if(targets.Count > 0) {
			Vector2 currentTarget = targets.Peek();

			//If we're done with this target, or not getting anywhere, dequeue the next target.
			float newDist = Vector2.Distance(transform.position, currentTarget);
			if (newDist < doneDist || newDist > minDist * diffDist)
            {
				transform.position = currentTarget;
				targets.Dequeue();
				origDist = Vector2.Distance(transform.position, currentTarget) * diffDist;
				minDist = float.PositiveInfinity;

                //Finished!
                if (targets.Count == 0)
                {
                    transform.position = currentTarget;
                    Node newPosition = getPathFinder().closestMostValidNode(transform.position);
                    node.Occupier = null;
                    newPosition.Occupier = this;
                    node = newPosition;

                    callbackFunction();
                }
            }
			//Otherwise continue making our way towards the target.
			else {
				transform.Rotate(Vector3.forward,
					getAbsoluteAngle(transform.up, currentTarget - (Vector2)(transform.position)));
				transform.position += transform.up.normalized * Time.deltaTime * doneDist * maxSpeed * 5.0f;
				if (newDist < minDist)
					minDist = newDist + diffDist;
			}
		}
	}

	/// <summary>
	/// Returns the absolute (0-360) degree angle between two vectors.
	/// </summary>
	/// <returns>The absolute (0-360) degree angle between two vectors.</returns>
	private float getAbsoluteAngle(Vector2 v1, Vector2 v2) {
		float angle = Vector2.Angle(v1, v2);

		if(AngleGreaterThan180(v1, v2))
			angle = 360 - angle;

		return angle;
	}

	//we use the cross product to detect the absolute heading.
	private bool AngleGreaterThan180(Vector2 v1, Vector2 v2) {
		//see http://stackoverflow.com/questions/7785601/detecting-if-angle-is-more-than-180-degrees
		// for calculation of angle > 180 degrees.

		return (v1.x * v2.y - v2.x * v1.y) < 0;
	}

    public abstract string name();

    public Unit(int clayAmount, int maximumWater, int bendinessFactor, int hardnessFactor, int attackRangeMin, int attackRangeMax, Node gridTile, bool e) {
		clay = clayAmount;
		currentWater = maxWater = maximumWater;
		bendiness = bendinessFactor;
		hardness = hardnessFactor;
		minAttackRange = attackRangeMin;
		maxAttackRange = attackRangeMax;
		enemy = e;

        node = gridTile;
	}

	// Getters
	public int getClay() {
		return clay;
	}

	public int getCurrentWater() {
		return currentWater;
	}

	public int getMaxWater() {
		return maxWater;
	}

	public int getBendiness() {
		return bendiness;
	}

	public int getHardness() {
		return hardness;
	}

	public int getMinAttackRange() {
		return minAttackRange;
	}

	public int getMaxAttackRange() {
		return maxAttackRange;
	}

	public Node getNode() {
		return node;
	}

	public bool isEnemy() {
		return enemy;
	}

	public bool isSelected() {
		return currentlySelected;
	}

	public void select() {
        UIManager.getUIManager().setDisplayedUnit(this);
		currentlySelected = true;
	}

	public void deselect()
    {
        UIManager.getUIManager().clearDisplay();
        currentlySelected = false;
	}

	public void setSpriteObject(GameObject g) {
		spriteObject = g;
	}

	// Takes the attack from an enemy unit, returning the counter-attack and attack results.
    // TODO: fix this weird attack problem
    // on counters it attacks itself?
	public List<AttackResult> takeAttackFrom(Unit enemy, int distance, bool firstAttack)
    {
        Attack atk = new Attack(enemy, this);

        List<AttackResult> attacks = new List<AttackResult>();
		AttackResult result = new AttackResult(HitType.Miss, 0, false, atk);
		AttackResult counter = new AttackResult(HitType.CannotCounter, 0, false);

        string statStr = "stats1 this " + ident() + ", clay: " + clay + ", endurance: " + currentWater;
        statStr += "\nstats1 enemy " + enemy.ident() + ", clay: " + enemy.clay + ", endurance: " + currentWater;
        statStr += "\nAttack info: " + atk;
        Debug.Log(statStr);

        Debug.Log("taking attack from: " + ident() + ", enemy doing this: " + enemy.ident());
        string playType = (isEnemy()) ? "enemy" : "player";

        if (Random.value <= atk.getHitChance())
        {
            result.setType(HitType.Hit);
            result.setDamageTaken(atk.getDamage());

            Debug.Log(playType + " was Hit!");

            if (Random.value <= atk.getCritChance())
            {
                clay -= (3 * atk.getDamage());

                result.setType(HitType.Crit);
                result.setDamageTaken(3 * atk.getDamage());
                Debug.Log("Crit!");
            }
            else
                clay -= atk.getDamage();
        }
        else
            Debug.Log(playType + " was Missed!");

		if(clay <= 0)
			result.setKilled(true);
		// Only counter if it is the first attack in a set and the enemy's range allows it
		else if(firstAttack && enemy.getMinAttackRange() <= distance && enemy.getMaxAttackRange() >= distance)
			counter = enemy.takeAttackFrom(this, distance, false)[0];

		// Add this enemy's result first, then the counter result
		attacks.Add(result);
		attacks.Add(counter);

        statStr = "\nstats2 this " + ident() + ", clay: " + clay + ", endurance: " + currentWater;
        statStr += "\nstats2 enemy " + enemy.ident() + ", clay: " + enemy.clay + ", endurance: " + currentWater;
        Debug.Log(statStr);
        return attacks;
	}

    public void Die()
    {
        node.Occupier = null;
        Object.Destroy(spriteObject);
    }

	// Attacks the given enemy Unit
    // Removes undo capability since attack is non-reverseable.
	public List<AttackResult> attack(Unit enemy, int distance) {
        canUndo = false;
		return enemy.takeAttackFrom(this, distance, true);
	}

    /// <summary>
    /// Sets the clay to this value.
    /// </summary>
    /// <param name="clayVal"></param>
    public void setClay(int clayVal)
    {
        clay = clayVal;
    }

    public int ident()
    {
        return transform.gameObject.GetInstanceID();
    }

    /// <summary>
    /// Sets the currentWater to this value.
    /// </summary>
    /// <param name="endurance"></param>
    public void setCurrentWater(int endurance)
    {
        currentWater = endurance;
    }
    
    public override int GetHashCode()
    {
        return transform.gameObject.GetInstanceID().GetHashCode();
    }

    public override bool Equals(object obj)
    {
        Unit ok = obj as Unit;
        if (ok == null)
            return false;
        return transform.gameObject.GetInstanceID() == ok.transform.gameObject.GetInstanceID();
    }
}
