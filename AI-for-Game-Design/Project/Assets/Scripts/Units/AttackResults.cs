using UnityEngine;
using System.Collections;

public enum HitType{Miss, Hit, Crit, CannotCounter};

public class AttackResult {
	
	private HitType type;
	private int damageTaken;
	private bool killed;

	// Creates a new Damage object representing the results of an attack
	public AttackResult(HitType t, int d, bool k) {
		type = t;
		damageTaken = d;
		killed = k;
	}

	// Getters
	public HitType getType() {
		return type;
	}

	public int getDamageTaken() {
		return damageTaken;
	}

	public bool wasKilled() {
		return killed;
	}

	// Setters
	public void setType(HitType t) {
		type = t;
	}

	public void setDamageTaken(int d) {
		damageTaken = d;
	}

	public void setKilled(bool k) {
		killed = k;
	}
}
