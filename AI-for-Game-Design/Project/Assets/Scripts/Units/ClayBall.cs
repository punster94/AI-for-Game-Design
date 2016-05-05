using UnityEngine;
using System.Collections.Generic;
using Graph;

public class ClayBallUnit : Unit {
    
	Sprite sprite = Resources.Load<Sprite>("ClayBall");
    
	// Constructs a BigGuyUnit given the GameObject it will relate to.
	public ClayBallUnit(GameObject parent, string name, Node gridTile, bool isEnemy) : base(1000, 50, 1, 0, 1, 1, gridTile, isEnemy){
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
        return "Clay Ball Unit";
    }
    
    // attacks then kills self.
    public override List<AttackResult> attack(Unit enemy, int distance)
    {
        Attack atk = new Attack(this, enemy);
        Attack count = new Attack(enemy, this);

        List<AttackResult> attacks = new List<AttackResult>();
        AttackResult result = new AttackResult(HitType.Hit, enemy.getClay(), true, atk);
        AttackResult counter = new AttackResult(HitType.Hit, getClay(), true, count);

        attacks.Add(result);
        attacks.Add(counter);
        return attacks;
    }
}
