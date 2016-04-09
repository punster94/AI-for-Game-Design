using UnityEngine;
using System.Collections;

public class AttackUtility {
	Unit attacker;
	Unit defender;
	int distanceOfAttack;
	double utility;
	double expectedDamage;
	double expectedCounterDamage;

	// Upon construction, an AttackUtility will store the utility of a given Unit attacker with a given Unit defender.
	// This is not dependent on the path the unit takes, so we can make two temporary units who do not need to be put in a node in the graph.
	// Just make sure that the units passed in have the necessary changes made to their water values.
	public AttackUtility(Unit a, Unit d, int dist) {
		attacker = a;
		defender = d;
		distanceOfAttack = dist;

		utility = expectedDamage = expectedCounterDamage = 0;

		if(a.getMinAttackRange() <= dist && a.getMaxAttackRange() >= dist)
			expectedDamage = calculateExpectedDamage(a, d);

		if(d.getMinAttackRange() <= dist && d.getMaxAttackRange() >= dist)
			expectedCounterDamage = calculateExpectedDamage(d, a);

		// Calculates a utility value using expected damage values
		utility = calculateUtility();
	}

	// TODO: carefully think out a better utility function here
	private double calculateUtility() {
		if(expectedCounterDamage == 0)
			return expectedDamage;
		return expectedDamage / expectedCounterDamage;
	}

	// Provides the expected damage resulting the attack of Unit a against Unit d
	private double calculateExpectedDamage(Unit a, Unit d) {
		double damage;
		Attack simulator = new Attack(a, d);

		// Chance of hitting * (chance of normal damage * normal damage + chance of critical hit * cricical damage)
		damage = simulator.getDamage() * (1 - simulator.getCritChance()) + 3 * simulator.getDamage() * simulator.getCritChance();
		damage *= simulator.getHitChance();

		return damage;
	}

	public Unit getAttacker() {
		return attacker;
	}

	public Unit getDefender() {
		return defender;
	}

	public int getDistanceOfAttack() {
		return distanceOfAttack;
	}

	public double getUtility() {
		return utility;
	}

	public double getExpectedDamage() {
		return expectedDamage;
	}

	public double getExpectedCounterDamage() {
		return expectedCounterDamage;
	}
}
