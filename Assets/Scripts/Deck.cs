using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Set Dynamically")]
    public PT_XMLReader XMLR;
    public Transform DeckAnchor;
    public List<Card> Cards;
    public List<Decorator> Decorators;
    public List<CardDefinition> CardDefinitions;
    public List<string> CardNames;
    public Dictionary<string, Sprite> DictionarySuits;

    // InitDeck ���������� ����������� Prospector, ����� ����� �����
    public void InitDeck(string deckXMLText)
    {
        ReadDeck(deckXMLText);
    }

    // ReadDeck ������ ��������� XML-���� � ������ ������ ����������� CardDefinition
    public void ReadDeck(string deckXMLText)
    {
        XMLR = new PT_XMLReader();   // ������� ����� ��������� PT_XMLReader
        XMLR.Parse(deckXMLText);     // ������������ ��� ���  ������ DeckXML

        // ����� ����������� ������, ����� ��������, ��� ������������ XMLR.
        // �� �������������� ����������� � ������ ������ XML ����������� � ���������� "�������� ����"
        string s = "xml[0] decorator[0] ";
        s += "type=" + XMLR.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + XMLR.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + XMLR.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + XMLR.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        // ��������� �������� <Decorator> ��� ���� ����
        Decorators = new List<Decorator>(); // ���������������� ������ ����������� Decorator
        // ������� ������ PT_XMLHashList ���� ��������� <Decorator> �� XML-�����
        PT_XMLHashList xDecos = XMLR.xml["xml"][0]["decorator"];
        Decorator decorator;
        for (int i = 0; i < xDecos.Count; i++)
        {
            // ��� ������� �������� <Decorator> � XML
            decorator = new Decorator(); // ������� ��������� Decorator
            // ����������� �������� �� <Decorator> � Decorator
            decorator.Type = xDecos[i].att("type");
            // decorator.Flip ������� �������� true, ���� �������� flip �������� ����� "1"
            decorator.Flip = (xDecos[i].att("flip") == "1");
            // �������� �������� float �� ��������� ���������
            decorator.Scale = float.Parse(xDecos[i].att("scale"));
            // Vector3 Loc ���������������� ��� [0, 0, 0], ������� ��� ������� ������ �������� ���
            decorator.Loc.x = float.Parse(xDecos[i].att("x"));
            decorator.Loc.y = float.Parse(xDecos[i].att("y"));
            decorator.Loc.z = float.Parse(xDecos[i].att("z"));
            // �������� decorator � ������ Decorators
            Decorators.Add(decorator);
        }

        // ��������� ���������� ��� �������, ������������ ����������� �����
        CardDefinitions = new List<CardDefinition>(); // ���������������� ������ ����
        // ������� ������ PT_XMLHashList ���� ��������� <Card> �� XML-�����
        PT_XMLHashList xCardDefinitions = XMLR.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefinitions.Count; i++)
        {
            // ��� ������� �������� <Card> ������� ��������� CardDefinition
            CardDefinition cDef = new CardDefinition();
            // �������� �������� �������� � �������� �� � cDef
            cDef.Rank = int.Parse(xCardDefinitions[i].att("rank"));
            // ������� ������ PT_XMLHashList ���� ��������� <pip> ������ ����� �������� <Card>
            PT_XMLHashList xPips = xCardDefinitions[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    // ������ ��� �������� <pip>
                    decorator = new Decorator();
                    // �������� <pip> � <Card> �������������� ������� Decorator
                    decorator.Type = "pip";
                    decorator.Flip = (xPips[j].att("flip") == "1");
                    decorator.Loc.x = float.Parse(xPips[j].att("x"));
                    decorator.Loc.y = float.Parse(xPips[j].att("y"));
                    decorator.Loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        decorator.Scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.Pips.Add(decorator);
                }
            }

            // ����� � ���������� (�����, ����, ������) ����� ������� face
            if (xCardDefinitions[i].HasAtt("face"))
            {
                cDef.Face = xCardDefinitions[i].att("face");
            }
            CardDefinitions.Add(cDef);
        }
    }
}
