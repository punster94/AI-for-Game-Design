using UnityEngine;
using System.Collections;
using Graph;
using System;

public class LongArmUnit : Unit {

	public static int framesInAnimation = 4;

	Sprite sprite = Resources.Load<Sprite>("LongArm");
    
	// Constructs a LongArmUnit given the GameObject it will relate to (remember, LongArmUnit and Unit are still both MonoBehavior classes)
	public LongArmUnit(GameObject parent, string name, Node gridTile, bool isEnemy) : base(10, 6, 6, 3, 1, 2, gridTile, isEnemy){
		GameObject g = new GameObject(name);
		g.transform.parent = parent.transform;
		g.transform.position = gridTile.getPos();
		g.AddComponent<SpriteRenderer>();
		g.GetComponent<SpriteRenderer>().sprite = sprite;
		setSpriteObject(g);

		if(isEnemy)
			g.GetComponent<SpriteRenderer>().color = new Color(200f, 0f, 0f);
	}
    

	public void die() {
		Debug.Log("I, the LongArmUnit, died. Bleh.");
	}

    public override string name()
    {
        return "Long Armed Unit";
    }

    private void updateAnimation() {
		animationFrame += 1;
		animationFrame %= framesInAnimation;

		if(animationFrame == 0) {
			Debug.Log("Frame 1");
		} else if(animationFrame == 1) {
			Debug.Log("Frame 2");
		} else if(animationFrame == 2) {
			Debug.Log("Frame 3");
		} else {
			Debug.Log("Frame 4");
		}
	}
}
