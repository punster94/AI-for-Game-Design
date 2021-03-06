﻿using UnityEngine;
using System.Collections;
using System;

public class Attack {
	private int damage;
	private float hitChance;
	private float critChance;
    private Unit atk;
    private Unit def;

    public Unit getDefender()
    {
        return def;
    }

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
    // Goal to max attack: maximize water.
	public Attack(Unit attacker, Unit defender) {
        atk = attacker;
        def = defender;
		damage = Mathf.Max(attacker.getClay() - defender.getHardness(), 0);
        // TODO: water: fix this to be more balanced.
        float waterVal = Mathf.Max(attacker.getMaxWater() / 2, attacker.getCurrentWater());
		hitChance = Mathf.Min((waterVal - defender.getBendiness()) / defender.getBendiness(), 1.0f);
		critChance = Mathf.Min((attacker.getBendiness() - defender.getBendiness()) / defender.getBendiness(), 1.0f);
	}
    
    public Unit getAttacker()
    {
        return atk;
    }

    public override string ToString()
    {
        float ev = getDamage() * (1 - getCritChance()) + 3 * getDamage() * getCritChance();
        ev *= getHitChance();
        return "Damage: " + damage + ", hitChance: " + hitChance + ", critChance: " + critChance + ", ev: " + ev;
    }
}
