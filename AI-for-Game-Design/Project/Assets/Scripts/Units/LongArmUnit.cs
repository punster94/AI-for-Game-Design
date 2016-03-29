using UnityEngine;
using System.Collections;

public class LongArmUnit : Unit {

	public static int framesInAnimation = 4;

	// Constructs a LongArmUnit given the GameObject it will relate to (remember, LongArmUnit and Unit are still both MonoBehavior classes)
	public LongArmUnit(GameObject g) : base(10, 6, 6, 3, 1, 2, g){}


	public void die() {
		Debug.Log("I, the LongArmUnit, died. Bleh.");
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
