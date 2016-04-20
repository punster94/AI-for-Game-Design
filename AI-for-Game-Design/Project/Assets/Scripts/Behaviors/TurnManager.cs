using UnityEngine;
using UnityEngine.UI;
using Graph;
using System.Collections.Generic;

class TurnManager 
{
    UnitAI ai;
    Button NextTurn;
    Button Cancel;
    Button Wait;
    State selectionState;
    PathFinder pathFinder;

    List<Unit> playerUnits;
    List<Unit> aiUnits;

    public TurnManager(PathFinder p, List<Unit> ally, List<Unit> enemy)
    {
        pathFinder = p;
        ai = new UnitAI(pathFinder);

        playerUnits = ally;
        aiUnits = enemy;

        NextTurn = UIManager.getUIManager().getNextTurnButton();
        Cancel = UIManager.getUIManager().getCancelButton();
        Wait = UIManager.getUIManager().getWaitButton();

        selectionState = State.OurTurnNoSelection;
        UIManager.getUIManager().ChangeButtonState(selectionState);

        toActAI = new Queue<Unit>();
    }
    
    Unit currentlySelectedUnit = null;
    bool unitSelected = false;

    bool aiTurn = true;
    bool moving = false;

    public void lockMovement()
    {
        moving = true;
    }

    public void finishedAttackCallback()
    {
        List<AttackResult> ars = currentAction.Attack(UnitAction.DontCallBack);
        foreach (AttackResult ar in ars)
        {
            Debug.Log("died: " + ar.ToString());
            if (ar.wasKilled())
            {
                Unit died = ar.target();
                if (died.isEnemy())
                {
                    aiUnits.Remove(died);
                }
                else
                {
                    playerUnits.Remove(died);
                }
                died.Die();
            }
        }
        moving = false;
    }

    // TODO: make attack not instant
    UnitAction currentAction;
    Queue<Unit> toActAI;
    

    private void nextTurn(List<Unit> toReset)
    {
        foreach (Unit u in toReset)
            u.resetTurn();
    }

    public void Update()
    {
        if (!moving)
        {
            if (aiTurn && toActAI.Count == 0)
            {
                selectionState = State.EnemyTurn;

                foreach (Unit unit in aiUnits)
                    toActAI.Enqueue(unit);

                if (toActAI.Count == 0)
                {
                    moving = true;
                    return;
                }
            }

            if (aiTurn)
            {
                Unit currentAIUnit = toActAI.Dequeue();
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);
                // turn ended
                if (toActAI.Count == 0)
                {
                    aiTurn = false;
                    nextTurn(playerUnits);
                }

                currentAction = ai.RunAI(currentAIUnit, playerUnits);
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);

                lockMovement();
                currentAction.Move(finishedAttackCallback);
            }

            if (!aiTurn && toActAI.Count == 0)
            {
                selectionState = State.EnemyTurn;

                foreach (Unit unit in playerUnits)
                    toActAI.Enqueue(unit);

                if (toActAI.Count == 0)
                {
                    moving = true;
                    return;
                }
            }

            if (!aiTurn)
            {
                Unit currentAIUnit = toActAI.Dequeue();
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);
                // turn ended
                if (toActAI.Count == 0)
                {
                    aiTurn = true;
                    nextTurn(aiUnits);
                }

                currentAction = ai.RunAI(currentAIUnit, aiUnits);
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);

                lockMovement();
                currentAction.Move(finishedAttackCallback);
            }
            else
            {
                selectionState = State.OurTurnNoSelection;

                if (Input.GetMouseButtonDown((int)MouseButton.left))
                {
                    Vector2 position = getMousePos();

                    if (positionInGraph(position))
                    {
                        Node clickedNode = pathFinder.closestMostValidNode(position);

                        if (unitSelected)
                        {
                            currentlySelectedUnit.deselect();
                            unitSelected = false;
                            selectionState = State.OurTurnNoSelection;
                        }

                        if (clickedNode.Occupied)
                        {
                            currentlySelectedUnit = clickedNode.Occupier;
                            currentlySelectedUnit.select();
                            unitSelected = true;
                            pathFinder.displayRangeOfUnit(currentlySelectedUnit, position);
                            if (currentlySelectedUnit.isEnemy())
                                selectionState = State.UnitActed;
                            else
                                selectionState = State.UnitFresh;
                        }
                        else
                        {
                            pathFinder.clearRangeDisplay();
                            selectionState = State.OurTurnNoSelection;
                        }
                    }
                }
                if (Input.GetMouseButtonDown((int)MouseButton.right))
                {
                    pathFinder.clearRangeDisplay();
                    selectionState = State.OurTurnNoSelection;
                }
            }

        }
        else
        {
            // disables all buttons while locked
            selectionState = State.EnemyTurn;


        }

        UIManager.getUIManager().ChangeButtonState(selectionState);
    }

    private Vector2 getMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private bool positionInGraph(Vector2 realPos)
    {
        Vector2 botLeft = pathFinder.getBottomLeftBound();
        Vector2 upRight = pathFinder.getTopRightBound();

        //NOTE: NOT the same as directly doing it, think integer division.
        Vector2 position = pathFinder.ArrPosToWorldSpace(pathFinder.WorldSpaceToArrPos(realPos));
        return position.x >= botLeft.x && position.y >= botLeft.y
            && position.x <= upRight.x && position.y <= upRight.y;
    }
}