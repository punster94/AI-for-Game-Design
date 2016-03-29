using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour {
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

	public Unit(int clayAmount, int maximumWater, int bendinessFactor, int hardnessFactor, int attackRangeMin, int attackRangeMax, GameObject g) {
		spriteObject = g;
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

	// Initializes tickers for animations and timimg
	void Start() {
		ticks = 0;
		animationFrame = 0;
	}

	// Generic update for updating animation of the given unit (may need to be moved to each individual unit type)
	void Update() {
		ticks %= ticksPerAnimation;

		// If it is time to change the units animation frame
		if(currentlySelected && ticks == 0) {
			updateAnimation();
		}

		ticks += 1;
	}

	private abstract void updateAnimation();
}
