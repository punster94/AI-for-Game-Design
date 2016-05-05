using UnityEngine;
using System.Collections;
using Graph;
using System;

public class RunnerUnit : Unit {
    
	Sprite sprite = Resources.Load<Sprite>("Runner");
    
	// Constructs a RunnerUnit given the GameObject it will relate to.
	public RunnerUnit(GameObject parent, string name, Node gridTile, bool isEnemy) : base(5, 24, 8, 2, 1, 1, gridTile, isEnemy){
		GameObject g = new GameObject(name);
		g.transform.parent = parent.transform;
		g.transform.position = gridTile.getPos();
		g.AddComponent<SpriteRenderer>();
		g.GetComponent<SpriteRenderer>().sprite = sprite;
		setSpriteObject(g);

		if(isEnemy)
			g.GetComponent<SpriteRenderer>().color = Color.yellow;
        else
            g.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
    }

    public override string name()
    {
        return "Runner Unit";
    }
}
