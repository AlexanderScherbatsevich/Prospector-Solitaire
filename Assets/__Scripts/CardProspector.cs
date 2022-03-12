using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}
public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    public eCardState state = eCardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();  //список других карт, не позвол€ющих эту перевернуть лицом вверх
    public int layoutID;  //определ€ет дл€ этой карты р€д в раскладке
    public SlotDef slotDef;  //этот класс хранит информацию из элемента <slot> в LayoutXML

    //определ€ет реакцию карт на щелчок мыши
    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this); 
        base.OnMouseUpAsButton();
    }
}
