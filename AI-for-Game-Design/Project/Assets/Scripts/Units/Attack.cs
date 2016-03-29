using UnityEngine;
using System.Collections;

public class Attack {
	private int damage;
	private float hitChance;
	private float critChance;

	// Getters
	public int getDamage() {
		return damage;
	}

	public float getHitChance() {
		return hitChance;
	}

	public float getCritChance() {
		return critChance;
	}

	// Calculates the damage, hit chance and critical chance of an attack from one unit to another
	public Attack(Unit attacker, Unit defender) {
		damage = Mathf.Max(attacker.getClay() - defender.getHardness(), 0);
		hitChance = Mathf.Min((attacker.getCurrentWater() - defender.getBendiness()) / defender.getBendiness(), 100f);
		critChance = Mathf.Min((attacker.getBendiness() - defender.getBendiness()) / defender.getBendiness(), 100f);
	}
}
