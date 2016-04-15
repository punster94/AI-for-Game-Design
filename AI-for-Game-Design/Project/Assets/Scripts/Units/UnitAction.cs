using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graph;

class UnitAction
{
    Unit unitRef;

    public UnitAction(Unit u)
    {
        unitRef = u;
    }

    public void SetAttack(Node n)
    {

    }

    public void SetMove(Node n)
    {

    }

    public void Attack(Action callBackFunc)
    {
        callBackFunc();
    }

    public void Move(Action callBackFunc)
    {
        ;
    }

    public void DoAllActions(Action callBackFunc)
    {
        Action del = () => { Attack(callBackFunc); };
        Move(del);
    }
}
