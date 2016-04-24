using UnityEngine;
using System.Collections;
using Graph;
using System;

public class LongRangeUnit : Unit {
    
	Sprite sprite = Resources.Load<Sprite>("LongRange");
    
	// Constructs a LongRangeUnit given the GameObject it will relate to.
	public LongRangeUnit(GameObject parent, string name, Node gridTile, bool isEnemy) : base(7, 7, 3, 2, 2, 4, gridTile, isEnemy){
		GameObject g = new GameObject(name);
		g.transform.parent = parent.transform;
		g.transform.position = gridTile.getPos();
		g.AddComponent<SpriteRenderer>();
		g.GetComponent<SpriteRenderer>().sprite = sprite;
		setSpriteObject(g);

		if(isEnemy)
			g.GetComponent<SpriteRenderer>().color = Color.magenta;
        else
            g.GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.4f, 0.4f);
    }

    public override string name()
    {
        return "Long Range Unit";
    }
}
