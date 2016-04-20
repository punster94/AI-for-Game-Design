using UnityEngine;
using System.Collections;
using Graph;
using System;

public class BigGuyUnit : Unit {
    
	Sprite sprite = Resources.Load<Sprite>("BigGuy");
    
	// Constructs a BigGuyUnit given the GameObject it will relate to.
	public BigGuyUnit(GameObject parent, string name, Node gridTile, bool isEnemy) : base(15, 6, 2, 5, 1, 1, gridTile, isEnemy){
		GameObject g = new GameObject(name);
		g.transform.parent = parent.transform;
		g.transform.position = gridTile.getPos();
		g.AddComponent<SpriteRenderer>();
		g.GetComponent<SpriteRenderer>().sprite = sprite;
		setSpriteObject(g);

		if(isEnemy)
			g.GetComponent<SpriteRenderer>().color = new Color(0, 0f, 1f);
        else
            g.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.1f, 0.1f);
    }

    public override string name()
    {
        return "Big Guy Unit";
    }
}
