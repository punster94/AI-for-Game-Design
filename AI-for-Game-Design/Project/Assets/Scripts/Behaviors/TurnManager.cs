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

        selectionState = State.OurTurnNoSelection;
        UIManager.getUIManager().ChangeButtonState(selectionState);

        toActAI = new Queue<Unit>();
    }

    void onNextTurn()
    {
        selectionState = State.EnemyTurn;
        UIManager.getUIManager().ChangeButtonState(selectionState);
        
        foreach (Unit u in playerUnits)
        {
            if (!u.hasMoved())
            {
                toActAI.Enqueue(u);
            }
        }

        // already moved all units, enemy's turn now.
        if (toActAI.Count == 0)
        {
            nextTurn(aiUnits);
            aiTurn = true;
        }

        // we won!
        if (aiUnits.Count == 0)
        {
            aiTurn = false;
            selectionState = State.OurTurnNoSelection;
            moving = true;
        }
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
        workingUnit.setMoved();
        autoSelectPlayerUnit();
    }
    
    bool unitSelected = false;

    bool aiTurn = false;
    bool moving = false;

    public void lockMovement()
    {
        moving = true;
    }

    // this is the player's callback for move-only actions.
    public void playerCallback()
    {
        UIManager.getUIManager().setDisplayedUnit(workingUnit);
        pathFinder.displayRangeOfUnit(workingUnit, workingUnit.getNode().getPos());
        selectionState = State.UnitMoved;
        unitSelected = true;
        moving = false;
    }

    // this is a callback that will attack the planned action square when called.
    public void attackSquare()
    {
        List<AttackResult> ars = currentAction.Attack(UnitAction.DontCallBack);
        bool weDied = false;
        foreach (AttackResult ar in ars)
        {
            Unit unit = ar.target();
            if (unit.isEnemy() != workingUnit.isEnemy())
            {
                pathFinder.highlightAttackedUnit(unit);
            }
            if (ar.wasKilled())
            {
                Unit died = unit;
                if (died.Equals(workingUnit))
                    weDied = true;
                diedList.Add(died);
            }
        }
        //cleanly display unit dead
        if (weDied)
        {
            workingUnit.setClay(0);
            workingUnit.setCurrentWater(0);
        }
        displayCurrentUnit(workingUnit);

        if (ars.Count > 0)
            setDelay(37);
        else
            setDelay(10);

        moving = false;
    }

    private List<Unit> diedList = new List<Unit>();

    // clears all units that died.
    public void clearDiedUnits()
    {
        foreach (Unit died in diedList)
        {
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
        wipeDisplay();
        diedList.Clear();
    }

    public void attackSquareAIcallback()
    {
        attackSquare();
        // turn ended
        if (toActAI.Count == 0)
        {
            // give extra time to see ending move of last turn.
            setDelay(37);
            aiTurn = !aiTurn;
            if (aiTurn)
                nextTurn(aiUnits);
            else
            {
                selectionState = State.OurTurnNoSelection;
                UIManager.getUIManager().ChangeButtonState(selectionState);
                nextTurn(playerUnits);
            }
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
    
    private int delayTime = 0;

    private void decDelay()
    {
        delayTime--;
    }

    private void setDelay(int delaySteps)
    {
        delayTime = delaySteps;
    }

    private bool delayDone()
    {
        return delayTime <= 0;
    }


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
        // player won!
        if (aiUnits.Count == 0)
        {
            pathFinder.clearHighlightedNodes();
            displayCurrentUnit(playerUnits[0]);
            UIManager.getUIManager().gameOver(true);
            selectionState = State.EnemyTurn;
            moving = true;
        }

        // ai won!
        if (playerUnits.Count == 0)
        {
            pathFinder.clearHighlightedNodes();
            displayCurrentUnit(aiUnits[0]);
            UIManager.getUIManager().gameOver(false);
            selectionState = State.EnemyTurn;
            moving = true;
        }

        if (delayDone() && !moving)
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
                            int range = Node.range(workingUnit.getNode(), clickedNode);

                            // determines if the click was in range if range was displayed.
                            bool clickedInRange = false;
                            if (workingUnit.hasMoved())
                                clickedInRange = range >= workingUnit.getMinAttackRange()
                                              && range <= workingUnit.getMaxAttackRange();

                            // don't auto select, the user clicked outside, deselect everything unless selected something else.
                            // in case where mistakenly left-clicked in range, don't deselect as the user would get confused.
                            if (!clickedInRange)
                            {
                                workingUnit.deselect();
                                unitSelected = false;
                                wipeDisplay();
                                selectionState = State.OurTurnNoSelection;
                            }
                        }


                        if (clickedNode.Occupied)
                        {
                            workingUnit = clickedNode.Occupier;
                            displayCurrentUnit(workingUnit);
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
                                displayCurrentUnit(workingUnit);
                                workingUnit.moveUnit(playerCallback, clickedNode);
                            }
                        }
                    }
                    // clicked out range, we disable unit and select next unit appropriately.
                    else
                    {
                        autoSelectPlayerUnit();
                    }
                }
            }
        }
        else
        {
            // disables all buttons while locked
            selectionState = State.EnemyTurn;

            if (!moving && !delayDone())
                decDelay();

            // if coming back from delay, update animation, dead units, and selection appropriately.
            if (delayDone())
            {
                if (!moving)
                    wipeDisplay();

                if (diedList.Count > 0)
                    clearDiedUnits();

                // now player turn, auto-select unit for player if possible.
                if (!aiTurn && !moving && toActAI.Count == 0)
                {
                    autoSelectPlayerUnit();
                }
            }
        }

        UIManager.getUIManager().ChangeButtonState(selectionState);
    }

    // attempts to auto-select the next valid player unit.
    private void autoSelectPlayerUnit()
    {
        selectionState = State.OurTurnNoSelection;
        wipeDisplay();

        bool switchedUnit = false;

        // if user clicked outside, disable auto select, as the user would think something crashed.
        if (workingUnit != null && !workingUnit.hasActed() && !workingUnit.hasMoved())
            return;

        // auto-select unit for player
        foreach (Unit u in playerUnits)
        {
            if (!u.hasActed() && !u.hasMoved())
            {
                if (unitSelected)
                {
                    workingUnit.deselect();
                }
                unitSelected = true;
                workingUnit = u;
                displayCurrentUnit(workingUnit);
                workingUnit.select();

                pathFinder.displayRangeOfUnit(workingUnit, workingUnit.getNode().getPos());
                selectionState = State.UnitFresh;
                switchedUnit = true;
                break;
            }
        }

        // auto next-turn
        if (!switchedUnit)
        {
            // give extra time to see ending move of last turn.
            setDelay(37);
            aiTurn = !aiTurn;
            if (aiTurn)
                nextTurn(aiUnits);
            else
            {
                selectionState = State.OurTurnNoSelection;
                UIManager.getUIManager().ChangeButtonState(selectionState);
                nextTurn(playerUnits);
            }
        }
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