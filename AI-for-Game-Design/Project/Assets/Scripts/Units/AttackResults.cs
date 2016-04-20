using UnityEngine;
using System.Collections;

public enum HitType{Miss, Hit, Crit, CannotCounter};

public class AttackResult {
	
	private HitType type;
	private int damageTaken;
	private bool killed;
    private Attack atk;

	// Creates a new Damage object representing the results of an attack
	public AttackResult(HitType t, int d, bool k, Attack atkRef = null) {
		type = t;
		damageTaken = d;
		killed = k;
        atk = atkRef;
	}
    
    public Unit target()
    {
        return atk.getDefender();
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

    public override string ToString()
    {
        if (atk != null)
            return ("attacking " + atk.getDefender().ident() + " " + (atk.getDefender().isEnemy() ? "AI" : "player") + " had " + damageTaken + " , cur clay=" + atk.getDefender().getClay() + " , waskilled = " + killed
                + " other side " + atk.getAttacker().ident() + " " + (atk.getAttacker().isEnemy() ? "AI" : "player") + " , cur clay=" + atk.getAttacker().getClay());
        return "";
    }
}
