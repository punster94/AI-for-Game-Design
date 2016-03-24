using UnityEngine;
using System.Collections;

// NOTE: This is the alternative move script which uses Unity Physics
// to update the object. NOT USED CURRENTLY.
// Kept in case we wish to enable sometime in future.

public class MoveScript : MonoBehaviour {
    private KeyCode upKey = KeyCode.UpArrow;
    private KeyCode downKey = KeyCode.DownArrow;
    private KeyCode leftKey = KeyCode.LeftArrow;
    private KeyCode rightKey = KeyCode.RightArrow;

    private Rigidbody2D subject;

    private float direction;
    private float speed;
    private float directionSpeed;

    public const float twoPi = Mathf.PI * 2;
    
	void Start()
    {
        speed = 6;
        subject = GetComponent<Rigidbody2D>();
        direction = subject.rotation * Mathf.Deg2Rad;
        directionSpeed = Mathf.PI / 12;
	}

    //manually edits the velocity, so we can have start, stop instead of acceleration.
    // if you did acceleration, you could use forces instead.
    void FixedUpdate()
    {
        direction = subject.rotation * Mathf.Deg2Rad;
	    if (Input.GetKey(upKey))
        {
            subject.velocity = convDirecspeedToVec(direction, speed);
        }
        else if (Input.GetKey(downKey))
        {
            subject.velocity = convDirecspeedToVec(direction, -speed);
        }
        else
        {
            subject.velocity = new Vector2(0, 0);
        }

        if (Input.GetKey(rightKey))
        {
            direction -= directionSpeed;
        }
        else if (Input.GetKey(leftKey))
        {
            direction += directionSpeed;
        }
        
        //Clamps the direction between 0 and 2pi
        direction = ClampDirection(direction);

        subject.rotation = direction * Mathf.Rad2Deg;
        subject.angularVelocity = 0;
    }

    //converts a direction-speed pair to a Vector2 xy speed coord pair.
    Vector2 convDirecspeedToVec(float direction, float speed)
    {
        //(x,y) -> (cos(dir), sin(dir))
        return new Vector2(Mathf.Cos(direction) * speed, Mathf.Sin(direction) * speed);
    }

    //Clamps a direction past two pi and less than 0.
    float ClampDirection(float direction)
    {
        if (direction >= twoPi)
            return 0;
        if (direction <= 0)
            return twoPi - Mathf.Epsilon;
        return direction;
    }
}
