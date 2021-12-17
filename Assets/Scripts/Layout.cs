using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����� SlotDef �� ��������� Monobehaviour, ������� ��� ���� �� ��������� ��������� ��������� ���� �� C#
[System.Serializable] // ������� ���������� SlotDef �������� � ���������� Unity
public class SlotDef
{
    public float X;
    public float Y;
    public bool FaceUp = false;
    public string LayerName = "Default";
    public int LayerID = 0;
    public int ID;
    public List<int> HiddenBy = new List<int>();
    public string Type = "slot";
    public Vector2 Stagger;
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader XMLR;
    public PT_XMLHashtable XML;
    public Vector2 Multiplier;
    // ������ SlotDef
    public List<SlotDef> SlotDefs;
    public SlotDef DrawPile;
    public SlotDef DiscardPile;
    // ������ ����� ���� �����
    public string[] SortingLayerNames = new string[] 
    { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    // ��� ������� ���������� ��� ������ ����� LayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        XMLR = new PT_XMLReader();
        XMLR.Parse(xmlText);        // ��������� XML
        XML = XMLR.xml["xml"][0];   // � ������������ XML ��� ��������� ������� � XML

        // ��������� ���������, ������������ ���������� ����� �������
        Multiplier.x = float.Parse(XML["multiplier"][0].att("x"));
        Multiplier.y = float.Parse(XML["multiplier"][0].att("y"));

        // ��������� �����
        SlotDef tSlotDef;
        // slotsX ������������� ��� ��������� ������ � ��������� <slot>
        PT_XMLHashList slotsX = XML["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSlotDef = new SlotDef(); // ������� ����� ��������� SlotDef
            if (slotsX[i].HasAtt("type"))
            {
                // ���� <slot> ����� ������� type, ��������� ���
                tSlotDef.Type = slotsX[i].att("type");
            }
            else
            {
                // ������ ���������� ��� ��� "slot" - ��� ��������� ����� � ����
                tSlotDef.Type = "slot";
            }

            // ������������� ��������� �������� � �������� ��������
            tSlotDef.X = float.Parse(slotsX[i].att("x"));
            tSlotDef.Y = float.Parse(slotsX[i].att("y"));
            tSlotDef.LayerID = int.Parse(slotsX[i].att("layer"));
            // ������������� ����� ���� LayerID � ����� LayerName
            tSlotDef.LayerName = SortingLayerNames[tSlotDef.LayerID];

            switch (tSlotDef.Type)
            {
                case "slot":
                    tSlotDef.FaceUp = (slotsX[i].att("faceup") == "1");
                    tSlotDef.ID = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSlotDef.HiddenBy.Add(int.Parse(s));
                        }
                    }

                    SlotDefs.Add(tSlotDef);
                    break;

                case "drawpile":
                    tSlotDef.Stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    DrawPile = tSlotDef;
                    break;

                case "discardpile":
                    DiscardPile = tSlotDef;
                    break;
            }
        }
    }
}
