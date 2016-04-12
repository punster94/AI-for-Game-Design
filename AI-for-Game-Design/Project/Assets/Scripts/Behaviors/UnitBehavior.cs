using UnityEngine;
using System.Collections;

public class UnitBehavior : MonoBehaviour {
	Unit unit;

	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
		unit.Update();
	}

	public void setUnit(Unit u) {
		unit = u;
		unit.transform = unit.spriteObject.transform;
	}
}
