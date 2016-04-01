using UnityEngine;
using System.Collections;

public abstract class Unit {
	// Fields for animation control
	public GameObject spriteObject;
	public int ticks;
	public static int ticksPerAnimation = 100;
	public int animationFrame;

	// Fields for action control
	public bool currentlySelected = false;

	private int clay;
	private int currentWater;
	private int maxWater;
	private int bendiness;
	private int hardness;

	private int minAttackRange;
	private int maxAttackRange;

	private int xPos;
	private int yPos;

	public Unit(int clayAmount, int maximumWater, int bendinessFactor, int hardnessFactor, int attackRangeMin, int attackRangeMax, int x, int y) {
		clay = clayAmount;
		currentWater = maxWater = maximumWater;
		bendiness = bendinessFactor;
		hardness = hardnessFactor;
		minAttackRange = attackRangeMin;
		maxAttackRange = attackRangeMax;
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

	public void setSpriteObject(GameObject g) {
		spriteObject = g;
	}

	// Takes the attack from an enemy unit, returning a list of attack results
	public ArrayList takeAttackFrom(Unit enemy, int distance, bool firstAttack = true) {
		Attack atk = new Attack(enemy, this);
		ArrayList attacks = new ArrayList();
		AttackResult result = new AttackResult(HitType.Miss, 0, false);
		AttackResult counter = new AttackResult(HitType.CannotCounter, 0, false);

		if(Random.value <= atk.getHitChance()) {
			result.setType(HitType.Hit);
			result.setDamageTaken(atk.getDamage());

			if(Random.value <= atk.getCritChance()) {
				clay -= (3 * atk.getDamage());

				result.setType(HitType.Crit);
				result.setDamageTaken(3 * atk.getDamage());
			}
			else
				clay -= atk.getDamage();
		}

		if(clay <= 0)
			result.setKilled(true);
		
		// Only counter if it is the first attack in a set and the enemy's range allows it
		else if(firstAttack && enemy.getMinAttackRange() <= distance && enemy.getMaxAttackRange() >= distance )
			enemy.takeAttackFrom(this, 1);

		return attacks;
	}

	// Attacks the given enemy Unit
	public ArrayList attack(Unit enemy, int distance) {
		return enemy.takeAttackFrom(this, distance);
	}
}
