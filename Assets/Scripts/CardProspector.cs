using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������������, ������������ ��� ����������, ������� ����� ��������� ��������� ��������������� ��������
public enum eCardState
{
    DrawPile,   // ������ ��� ��������� ����
    Tableau,    // ������ ����� "����"
    Target,     // "������� �����" - �������� ����� �� ������� ������ ���������� ����
    Discard     // ������ ���������� ����
}

public class CardProspector : Card   // CardProspector ������ ��������� ����� Card
{
    [Header("Set Dynamically: CardProspector")]
    // ��� ������������ ������������ eCardState
    public eCardState State = eCardState.DrawPile;
    // HiddenBy - ������ ������ ����, �� ����������� ����������� ��� ����� �����
    public List<CardProspector> HiddenBy = new List<CardProspector>();
    // LayoutID ���������� ��� ���� ����� ��� � ���������
    public int LayoutID;
    // ����� SlotDef ������ ���������� �� �������� <slot> � LayoutXML
    public SlotDef SlotDef;

    // ���������� ������� ���� �� ������ ����
    override public void OnMouseUpAsButton()
    {
        // ������� ����� CardClicked(this) �������-�������� Prospector
        Prospector.S.CardClicked(this);
        // � ����� ������ ����� ������ � ������� ������ (Card.cs)
        base.OnMouseUpAsButton();
    }
}
