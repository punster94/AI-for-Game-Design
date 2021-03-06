﻿using UnityEngine;
using System.Collections;

public class AttackRound {
	Unit attacker;
	Unit defender;
	float utility;
	int expectedDamage;
	int expectedCounterDamage;

    bool dieAtk;
    bool dieDef;

    int waterAtk;
    int clayDef;

	// Sets damage done by one round of attacking.
    // Must be re-set after doing this; assumes within countering range.
	public AttackRound(Unit attackerIn, int attackerWaterCost, Unit defenderIn) {
        attacker = attackerIn;
		defender = defenderIn;

        utility = expectedDamage = expectedCounterDamage = 0;
        dieDef = dieAtk = false;

        // Store current stats.
        waterAtk = attacker.getCurrentWater();
        clayDef = defender.getClay();

        attacker.setCurrentWater(waterAtk - attackerWaterCost);

		expectedDamage = calculateExpectedDamage(attacker, defender);

        // Assume defender looses appropriate amount of clay before counter.
        if ((clayDef - expectedDamage) > 0)
        {
            defender.setClay(clayDef - expectedDamage);
            expectedCounterDamage = calculateExpectedDamage(defender, attacker);
        }
        else
        {
            defender.setClay(0);
            dieDef = true;
        }

        if ((attacker.getClay() - expectedDamage) < 0)
        {
            dieAtk = true;
        }

		// Calculates a utility value using expected damage values
		utility = calculateUtility();
    }

    // Resets values.
    public void resetBack()
    {
        attacker.setCurrentWater(waterAtk);
        defender.setClay(clayDef);
    }
    
    public bool attackerDies()
    {
        return dieAtk;
    }

    public bool defenderDies()
    {
        return dieDef;
    }

    private float calculateUtility() {
		if (expectedCounterDamage  <= 1)
			return expectedDamage;
		return ((float) (expectedDamage)) / expectedCounterDamage;
	}

	// Provides the expected damage resulting the attack.
	private int calculateExpectedDamage(Unit atk, Unit def) {
		float damage;
		Attack simulator = new Attack(atk, def);

		// Chance of hitting * (chance of normal damage * normal damage + chance of critical hit * cricical damage)
		damage = simulator.getDamage() * (1 - simulator.getCritChance()) + 3 * simulator.getDamage() * simulator.getCritChance();
		damage *= simulator.getHitChance();

		return Mathf.RoundToInt(damage);
	}

	public Unit getAttacker() {
		return attacker;
	}

	public Unit getDefender() {
		return defender;
	}

	public double getUtility() {
		return utility;
	}

	public int getExpectedDamage() {
		return expectedDamage;
	}

	public int getExpectedCounterDamage() {
		return expectedCounterDamage;
	}
}
