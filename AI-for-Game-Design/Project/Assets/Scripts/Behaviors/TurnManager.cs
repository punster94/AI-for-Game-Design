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

        NextTurn.onClick.AddListener(onNextTurn);
        Cancel.onClick.AddListener(onCancel);
        Wait.onClick.AddListener(onWait);

        selectionState = State.UnitFresh;
        UIManager.getUIManager().ChangeButtonState(selectionState);

        toActAI = new Queue<Unit>();
    }

    void onNextTurn()
    {
        selectionState = State.EnemyTurn;
        UIManager.getUIManager().ChangeButtonState(selectionState);
        aiTurn = true;
        // Add remaining units to queue that have not moved
    }

    void onCancel()
    {
        selectionState = State.UnitFresh;
        UIManager.getUIManager().ChangeButtonState(selectionState);
        unitSelected = true;
        workingUnit.undoIfPossible();
        wipeDisplay();
        displayCurrentUnit(workingUnit);
    }
 
    void onWait()
    {
        selectionState = State.UnitActed;
        unitSelected = false;
        UIManager.getUIManager().ChangeButtonState(selectionState);
        workingUnit.hasActed(true);
    }
    
    bool unitSelected = false;

    bool aiTurn = false;
    bool moving = false;

    public void lockMovement()
    {
        moving = true;
    }

    public void playerCallback()
    {
        displayCurrentUnit(workingUnit);
        selectionState = State.UnitMoved;
        unitSelected = true;
        moving = false;
    }

    public void attackSquare()
    {
        List<AttackResult> ars = currentAction.Attack(UnitAction.DontCallBack);
        bool weDied = false;

        foreach (AttackResult ar in ars)
        {
            Debug.Log("died: " + ar.ToString());
            if (ar.wasKilled())
            {
                Unit died = ar.target();
                if (died.Equals(workingUnit))
                {
                    wipeDisplay();
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
            else
            {
                Unit didntDie = ar.target();
                if (didntDie.isEnemy() != workingUnit.isEnemy())
                {
                    pathFinder.highlightAttackedUnit(didntDie);
                }
            }
        }
        if (!weDied)
        {
            displayCurrentUnit(workingUnit);
        }

        if (ars.Count > 0)
            delayTime = 37;
        else
            delayTime = 3;

        moving = false;
    }

    public void attackSquareAIcallback()
    {
        attackSquare();
        // turn ended
        if (toActAI.Count == 0)
        {
            aiTurn = !aiTurn;
            if (aiTurn)
                nextTurn(aiUnits);
            else
                nextTurn(playerUnits);
        }
    }

    private void wipeDisplay()
    {
        UIManager.getUIManager().clearDisplay();
        pathFinder.clearHighlightedNodes();
        pathFinder.clearRangeDisplay();
    }

    private void displayCurrentUnit(Unit u)
    { 
        UIManager.getUIManager().setDisplayedUnit(u);
        pathFinder.displayRangeOfUnit(u, u.getNode().getPos());
        pathFinder.highlightSelectedUnit(u);
    }

    // TODO: make attack not instant
    int delayTime = 0;
    UnitAction currentAction;
    Unit workingUnit;
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
                workingUnit = toActAI.Dequeue();
                wipeDisplay();
                displayCurrentUnit(workingUnit);

                currentAction = ai.RunAI(workingUnit, playerUnits);
                UIManager.getUIManager().setDisplayedUnit(workingUnit);

                lockMovement();
                currentAction.Move(attackSquareAIcallback);
            }

            /*
            if (!aiTurn && toActAI.Count == 0)
            {
                //selectionState = State.EnemyTurn;

                foreach (Unit unit in playerUnits)
                    toActAI.Enqueue(unit);

                if (toActAI.Count == 0)
                {
                    moving = true;
                    UIManager.getUIManager().clearDisplay();
                    pathFinder.clearRangeDisplay();
                    return;
                }
            }*/

            if (!aiTurn && toActAI.Count > 0)
            {
                workingUnit = toActAI.Dequeue();
                wipeDisplay();
                displayCurrentUnit(workingUnit);

                currentAction = ai.RunAI(workingUnit, aiUnits);
                UIManager.getUIManager().setDisplayedUnit(workingUnit);

                lockMovement();
                currentAction.Move(attackSquareAIcallback);
            }
            else if (!aiTurn)
            {
                //select logic
                if (Input.GetMouseButtonDown((int)MouseButton.left))
                {
                    Vector2 position = getMousePos();

                    if (positionInGraph(position))
                    {
                        Node clickedNode = pathFinder.closestMostValidNode(position);

                        if (unitSelected)
                        {
                            workingUnit.deselect();
                            unitSelected = false;
                            wipeDisplay();
                            selectionState = State.OurTurnNoSelection;
                        }

                        if (clickedNode.Occupied)
                        {
                            workingUnit = clickedNode.Occupier;
                            workingUnit.select();
                            unitSelected = true;
                            pathFinder.displayRangeOfUnit(workingUnit, position);
                            if (workingUnit.isEnemy())
                                selectionState = State.UnitActed;
                            else
                            {
                                if (workingUnit.hasActed())
                                    selectionState = State.OurTurnNoSelection;
                                else
                                    selectionState = State.UnitFresh;
                            }
                        }
                        else
                        {
                            pathFinder.clearRangeDisplay();
                            selectionState = State.OurTurnNoSelection;
                        }
                    }
                }
                // attack/move logic
                if (Input.GetMouseButtonDown((int)MouseButton.right))
                {
                    Vector2 mousePos = getMousePos();

                    if (positionInGraph(mousePos) && unitSelected && !workingUnit.isEnemy() && !workingUnit.hasActed())
                    {
                        wipeDisplay();
                        Node clickedNode = pathFinder.closestMostValidNode(mousePos);

                        // attacking
                        if (workingUnit.hasMoved())
                        {
                            int range = Node.range(workingUnit.getNode(), clickedNode);
                            // can attack
                            if (clickedNode.Occupied && clickedNode.Occupier.isEnemy() == true
                             && range >= workingUnit.getMinAttackRange()
                             && range <= workingUnit.getMaxAttackRange())
                            {
                                currentAction = new UnitAction(workingUnit, clickedNode.Occupier, workingUnit.getNode());
                                currentAction.Move(attackSquare);
                                selectionState = State.UnitActed;
                            }
                        }
                        // moving
                        else
                        {
                            selectionState = State.EnemyTurn;

                            Queue<Node> path = new Queue<Node>();
                            double cost = pathFinder.AStar(path, workingUnit.getNode(), clickedNode);
                            if (cost <= workingUnit.getCurrentWater())
                            {
                                moving = true;
                                workingUnit.moveUnit(playerCallback, clickedNode);
                            }
                        }
                        
                    }
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