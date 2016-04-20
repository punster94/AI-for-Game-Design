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

    public void finishedMoveCallbackAI()
    {
        List<AttackResult> ars = currentAction.Attack(UnitAction.DontCallBack);
        bool weDied = false;

        foreach (AttackResult ar in ars)
        {
            Debug.Log("died: " + ar.ToString());
            if (ar.wasKilled())
            {
                Unit died = ar.target();
                if (died.Equals(currentAIUnit))
                {
                    UIManager.getUIManager().clearDisplay();
                    weDied = true;
                }
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
        // turn ended
        if (toActAI.Count == 0)
        {
            aiTurn = !aiTurn;
            if (aiTurn)
                nextTurn(aiUnits);
            else
                nextTurn(playerUnits);
        }
        if (!weDied)
        {
            UIManager.getUIManager().setDisplayedUnit(currentAIUnit);
            pathFinder.displayRangeOfUnit(currentAIUnit, currentAIUnit.getNode().getPos());
        }

        if (ars.Count > 0)
            delayTime = 37;
        else
            delayTime = 3;
        moving = false;
    }

    // TODO: make attack not instant
    int delayTime = 0;
    UnitAction currentAction;
    Unit currentAIUnit;
    Queue<Unit> toActAI;
    

    private void nextTurn(List<Unit> toReset)
    {
        foreach (Unit u in toReset)
            u.resetTurn();
    }

    public void Update()
    {
        if (delayTime <= 0 && !moving)
        {
            if (aiTurn && toActAI.Count == 0)
            {
                selectionState = State.EnemyTurn;

                foreach (Unit unit in aiUnits)
                    toActAI.Enqueue(unit);

                if (toActAI.Count == 0)
                {
                    moving = true;
                    UIManager.getUIManager().clearDisplay();
                    pathFinder.clearRangeDisplay();
                    return;
                }
            }

            if (aiTurn)
            {
                currentAIUnit = toActAI.Dequeue();
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);
                pathFinder.displayRangeOfUnit(currentAIUnit, currentAIUnit.getNode().getPos());

                currentAction = ai.RunAI(currentAIUnit, playerUnits);
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);

                lockMovement();
                currentAction.Move(finishedMoveCallbackAI);
            }

            if (!aiTurn && toActAI.Count == 0)
            {
                selectionState = State.EnemyTurn;

                foreach (Unit unit in playerUnits)
                    toActAI.Enqueue(unit);

                if (toActAI.Count == 0)
                {
                    moving = true;
                    UIManager.getUIManager().clearDisplay();
                    pathFinder.clearRangeDisplay();
                    return;
                }
            }

            if (!aiTurn)
            {
                currentAIUnit = toActAI.Dequeue();
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);
                pathFinder.displayRangeOfUnit(currentAIUnit, currentAIUnit.getNode().getPos());

                currentAction = ai.RunAI(currentAIUnit, aiUnits);
                UIManager.getUIManager().setDisplayedUnit(currentAIUnit);

                lockMovement();
                currentAction.Move(finishedMoveCallbackAI);
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

            if (delayTime > 0)
                delayTime--;
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