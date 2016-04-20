using UnityEngine;
using UnityEngine.UI;

public enum State { UnitFresh, UnitMoved, UnitActed, EnemyTurn, OurTurnNoSelection };

class UIManager
{
    static private UIManager uiRef;

    private GameObject canvasRef;
    private Text UnitName;
    private Text UnitClay;
    private Text UnitWater;
    private Text UnitBendiness;
    private Text UnitHardness;
    private Text UnitRangeNotation;
    
    private Button NextTurn;
    private Button Cancel;
    private Button Wait;

    private UIManager()
    {
        canvasRef = GameObject.Find("Canvas");
        Transform panel = canvasRef.transform.Find("Panel");
        UnitName = getRefFromPanel("UnitName", panel);
        UnitClay = getRefFromPanel("UnitClay", panel);
        UnitWater = getRefFromPanel("UnitWater", panel);
        UnitBendiness = getRefFromPanel("UnitBendiness", panel);
        UnitHardness = getRefFromPanel("UnitHardness", panel);
        UnitRangeNotation = getRefFromPanel("UnitRangeNotation", panel);
        
        NextTurn = getButton("NextTurn");
        Cancel = getButton("Cancel");
        Wait = getButton("Wait");
    }

    private Text getRefFromPanel(string name, Transform panel)
    {
        return panel.Find(name).gameObject.GetComponent<Text>();
    }

    public static UIManager getUIManager()
    {
        if (uiRef == null)
            uiRef = new UIManager();
        return uiRef;
    }

    public void ChangeButtonState(State b)
    {
        switch(b)
        {
            case State.UnitFresh:
                setButtonState(true, false, true);
                break;
            case State.UnitMoved:
                setButtonState(true, true, true);
                break;
            case State.UnitActed:
                setButtonState(false, false, true);
                break;
            case State.EnemyTurn:
                setButtonState(false, false, false);
                break;
            case State.OurTurnNoSelection:
                setButtonState(false, false, true);
                break;
        }
    }

    private void setButtonState(bool wait, bool cancel, bool nextTurn)
    {
        Wait.interactable = wait;
        Cancel.interactable = cancel;
        NextTurn.interactable = nextTurn;
    }

    private Button getButton(string name)
    {
        return canvasRef.transform.Find(name).GetComponent<Button>();
    }

    public Button getNextTurnButton()
    {
        return NextTurn;
    }

    public Button getCancelButton()
    {
        return Cancel;
    }

    public Button getWaitButton()
    {
        return Wait;
    }

    public void setDisplayedUnit(Unit uRef)
    {
        UnitName.text = "Name: " + uRef.name();
        UnitName.color = (uRef.isEnemy()) ? Color.red : Color.black;
        UnitClay.text = "Clay: " + uRef.getClay();
        UnitWater.text = "Water: " + uRef.getCurrentWater();
        UnitBendiness.text = "Bendiness: " + uRef.getBendiness();
        UnitHardness.text = "Hardness: " + uRef.getHardness();
        UnitRangeNotation.text = "Range: [" + uRef.getMinAttackRange() + ", " + uRef.getMaxAttackRange() + "]";
    }
    
    public void clearDisplay()
    {
        UnitName.text = "Name: ";
        UnitName.color = Color.black;
        UnitClay.text = "Clay: ";
        UnitWater.text = "Water: ";
        UnitBendiness.text = "Bendiness: ";
        UnitHardness.text = "Hardness: ";
        UnitRangeNotation.text = "Range: [ , ]";
    }
}
