using UnityEngine;
using System.Collections;
using Graph;
using System;

public class LongArmUnit : Unit {
    
	Sprite sprite = Resources.Load<Sprite>("LongArm");
    
	// Constructs a LongArmUnit given the GameObject it will relate to.
	public LongArmUnit(GameObject parent, string name, Node gridTile, bool isEnemy) : base(10, 12, 6, 3, 1, 2, gridTile, isEnemy){
		GameObject g = new GameObject(name);
		g.transform.parent = parent.transform;
		g.transform.position = gridTile.getPos();
		g.AddComponent<SpriteRenderer>();
		g.GetComponent<SpriteRenderer>().sprite = sprite;
		setSpriteObject(g);

		if(isEnemy)
			g.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
        else
            g.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f);
    }

    public override string name()
    {
        return "Long Armed Unit";
    }
}
