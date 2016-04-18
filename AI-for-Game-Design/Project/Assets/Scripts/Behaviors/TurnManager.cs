using UnityEngine;
using UnityEngine.UI;
using Graph;

class TurnManager 
{
    UnitAI ai;
    Button NextTurn;
    Button Cancel;
    Button Wait;
    ButtonState selectionState;
    PathFinder pathFinder;

    public TurnManager(PathFinder p)
    {
        pathFinder = p;
        ai = new UnitAI(pathFinder);

        NextTurn = UIManager.getUIManager().getNextTurnButton();
        Cancel = UIManager.getUIManager().getCancelButton();
        Wait = UIManager.getUIManager().getWaitButton();

        selectionState = ButtonState.OurTurnNoSelection;
        UIManager.getUIManager().ChangeButtonState(selectionState);
    }
    
    Unit currentlySelectedUnit = null;
    bool unitSelected = false;

    bool turn = true;

    public void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.left))
        {
            Vector2 position = getMousePos();

            if (clickInGraph(position))
            {
                Node clickedNode = pathFinder.closestMostValidNode(position);

                if (unitSelected)
                {
                    currentlySelectedUnit.deselect();
                    unitSelected = false;
                }

                if (clickedNode.Occupied)
                {
                    currentlySelectedUnit = clickedNode.Occupier;
                    currentlySelectedUnit.select();
                    unitSelected = true;
                    pathFinder.displayRangeOfUnit(currentlySelectedUnit, position);
                }
                else
                {
                    pathFinder.clearRangeDisplay();
                }
            }
        }
        if (Input.GetMouseButtonDown((int)MouseButton.right))
            pathFinder.clearRangeDisplay();
    }

    private Vector2 getMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private bool clickInGraph(Vector2 position)
    {
        //TODO: fix bounding box issues with bottom left & top right edges of map
        Vector2 botLeft = pathFinder.getBottomLeftBound();
        Vector2 upRight = pathFinder.getTopRightBound();
        return position.x >= botLeft.x && position.y >= botLeft.y
            && position.x <= upRight.x && position.y <= upRight.y;
    }
}