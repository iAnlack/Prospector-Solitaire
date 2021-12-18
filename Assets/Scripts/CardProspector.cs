using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Перечисление, определяющие тип переменной, которая может принимать несколько предопределённых значений
public enum eCardState
{
    DrawPile,   // Стопка для свободных карт
    Tableau,    // Стопки самих "шахт"
    Target,     // "Целевая карта" - активная карта на вершине стопки сброшенных карт
    Discard     // Стопка сброшенных карт
}

public class CardProspector : Card   // CardProspector должен расширять класс Card
{
    [Header("Set Dynamically: CardProspector")]
    // Так используется перечисление eCardState
    public eCardState State = eCardState.DrawPile;
    // HiddenBy - список других карт, не позволяющих перевернуть эту лицом вверх
    public List<CardProspector> HiddenBy = new List<CardProspector>();
    // LayoutID определяет для этой карты ряд в раскладке
    public int LayoutID;
    // Класс SlotDef хранит информацию из элемента <slot> в LayoutXML
    public SlotDef SlotDef;

    // Определяет реакцию карт на щелчок мыши
    override public void OnMouseUpAsButton()
    {
        // Вызвать метод CardClicked(this) объекта-одиночки Prospector
        Prospector.S.CardClicked(this);
        // а также версию этого метода в базовом классе (Card.cs)
        base.OnMouseUpAsButton();
    }
}
