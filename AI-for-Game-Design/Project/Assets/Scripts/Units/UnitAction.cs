using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graph;

class UnitAction
{
    /// <summary>
    /// Void function that does nothing when called.
    /// Use this instead of null Action... or not.
    /// </summary>
    public static Action DontCallBack = () => { };
    
    Unit unitRef, enemyUnit;
    Node moveNode;

    /// <summary>
    /// Creates an action that causes a unit to 
    /// stay in place and not attack anything.
    /// </summary>
    /// <param name="u">The unit to create the action for.</param>
    /// <returns>The UnitAction.</returns>
    public static UnitAction DoNothing(Unit u)
    {
        return new UnitAction(u, null, u.getNode());
    }

    /// <summary>
    /// Creates a unitAction for a subject.
    /// </summary>
    /// <param name="subject">The subject to create an action for.</param>
    /// <param name="enemyToAttack"></param>
    /// <param name="movePosition"></param>
    public UnitAction(Unit subject, Unit enemyToAttack, Node movePosition)
    {
        unitRef = subject;
        enemyUnit = enemyToAttack;
        moveNode = movePosition;
    }

    /// <summary>
    /// Tries to undo an action. If successful, returns true, else false.
    /// </summary>
    /// <returns>If successful, returns true, else false.</returns>
    public bool Undo()
    {
        return unitRef.undoIfPossible();
    }

    /// <summary>
    /// Runs this action's attack.
    /// </summary>
    /// <param name="callbackFuncOnDone">The void action to run when done.</param>
    public List<AttackResult> Attack(Action callbackFuncOnDone)
    {
        List<AttackResult> results = new List<AttackResult>();
        if (enemyUnit != null)
        {
            results.AddRange(unitRef.attack(enemyUnit, Node.range(unitRef.getNode(), enemyUnit.getNode())));
        }
        
        if (callbackFuncOnDone != null)
            callbackFuncOnDone();

        return results;
    }

    /// <summary>
    /// Runs this action's move.
    /// </summary>
    /// <param name="callbackFuncOnDone">The void action to run when done.</param>
    public void Move(Action callbackFuncOnDone)
    {
        if (callbackFuncOnDone != null)
            unitRef.moveUnit(callbackFuncOnDone, moveNode);
        else
            unitRef.moveUnit(callbackFuncOnDone, moveNode);
    }
}
