using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool StartFaceUp = false;
    // �����
    public Sprite SuitClub;
    public Sprite SuitDiamond;
    public Sprite SuitHeart;
    public Sprite SuitSpade;

    public Sprite[] FaceSprites;
    public Sprite[] RankSprites;

    public Sprite CardBack;
    public Sprite CardBackGold;
    public Sprite CardFront;
    public Sprite CardFrontGold;

    // �������
    public GameObject PrefabCard;
    public GameObject PrefabSprite;

    [Header("Set Dynamically")]
    public PT_XMLReader XMLR;
    public List<string> CardNames;
    public List<Card> Cards;
    public List<Decorator> Decorators;
    public List<CardDefinition> CardDefinitions;
    public Transform DeckAnchor;
    public Dictionary<string, Sprite> DictionarySuits;

    // InitDeck ���������� ����������� Prospector, ����� ����� �����
    public void InitDeck(string deckXMLText)
    {
        // ������� ����� �������� ��� ���� ������� �������� Card � ��������
        if (GameObject.Find("Deck") == null)
        {
            GameObject anchorGO = new GameObject("Deck");
            DeckAnchor = anchorGO.transform;
        }

        // ���������������� ������� �� ��������� ������� ������
        DictionarySuits = new Dictionary<string, Sprite>()
        {
            {"C", SuitClub },
            {"D", SuitDiamond },
            {"H", SuitHeart },
            {"S", SuitSpade }
        };

        ReadDeck(deckXMLText);

        MakeCards();
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

    // �������� CardDefinition �� ������ �������� ����������� (�� 1 �� 14 - �� ���� �� ������)
    public CardDefinition GetCardDefinitionByRank(int rank)
    {
        // ����� �� ���� ������������ CardDefinition
        foreach (CardDefinition cardDefinition in CardDefinitions)
        {
            // ���� ����������� ��������� ������� ��� �����������
            if (cardDefinition.Rank == rank)
            {
                return (cardDefinition);
            }
        }

        return null;
    }

    // ������ ������� ������� ����
    public void MakeCards()
    {
        // CardNames ����� ��������� ����� ����������������� ����
        // ������ ����� ����� 14 �������� ����������� (�������� ��� ���� (Clubs): �� C1 �� C14)
        CardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                CardNames.Add(s + (i + 1));
            }
        }

        // ������� ������ �� ����� �������
        Cards = new List<Card>();

        // ������ ��� ������ ��� ��������� ����� ����
        for (int i = 0; i < CardNames.Count; i++)
        {
            // ������� ����� � �������� � � ������
            Cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        // ������� ����� ������� ������ � ������
        GameObject cGO = Instantiate(PrefabCard) as GameObject;
        // ��������� transform.parent ����� ����� � ������������ � ������ ��������
        cGO.transform.parent = DeckAnchor;
        Card card = cGO.GetComponent<Card>(); // �������� ��������� Card

        // ��� ������ ����������� ����� � ���������� ���
        cGO.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        // ��������� �������� ��������� �����
        card.name = CardNames[cNum];
        card.Suit = card.name[0].ToString();
        card.Rank = int.Parse(card.name.Substring(1));
        if (card.Suit == "D" || card.Suit == "H")
        {
            card.ColS = "Red";
            card.Color = Color.red;
        }

        // �������� CardDefinition ��� ���� �����
        card.Definition = GetCardDefinitionByRank(card.Rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    // ��������� ������� ���������� ������������ ���������������� ��������
    private Sprite _tSprite = null;
    private GameObject _tGameObject = null;
    private SpriteRenderer _tSpriteRenderer = null;

    private void AddDecorators(Card card)
    {
        // �������� ����������
        foreach (Decorator decorator in Decorators)
        {
            if (decorator.Type == "Suit")
            {
                // ������� ��������� �������� ������� �������
                _tGameObject = Instantiate(PrefabSprite) as GameObject;
                // �������� ������ �� ��������� SpriteRenderer
                _tSpriteRenderer = _tGameObject.GetComponent<SpriteRenderer>();
                // ���������� ������ �����
                _tSpriteRenderer.sprite = DictionarySuits[card.Suit];
            }
            else
            {
                _tGameObject = Instantiate(PrefabSprite) as GameObject;
                _tSpriteRenderer = _tGameObject.GetComponent<SpriteRenderer>();
                // �������� ������ ��� ����������� �����������
                _tSprite = RankSprites[card.Rank];
                // ���������� ������ ����������� � SpriteRenderer
                _tSpriteRenderer.sprite = _tSprite;
                // ���������� ����, ��������������� �����
                _tSpriteRenderer.color = card.Color;
            }

            // ��������� ������� ��� ������
            _tSpriteRenderer.sortingOrder = 1;
            // ������� ������ �������� �� ��������� � �����
            _tGameObject.transform.SetParent(card.transform);
            // ���������� localPosition, ��� ���������� � DeckXML
            _tGameObject.transform.localPosition = decorator.Loc;
            // ����������� ������, ���� ����������
            if (decorator.Flip)
            {
                // ������� ������� �� 180� ������������ ��� z-axis
                _tGameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            // ���������� �������, ����� ��������� ������ �������
            if (decorator.Scale != 1)
            {
                _tGameObject.transform.localScale = Vector3.one * decorator.Scale;
            }

            // ���� ��� ����� �������� ������� ��� �����������
            _tGameObject.name = decorator.Type;
            // �������� ���� ������� ������ � ����������� � ������ card.DecoGOs
            card.DecoGOs.Add(_tGameObject);
        }
    }

    private void AddPips(Card card)
    {
        // ��� ������� ������ � �����������...
        foreach (Decorator pip in card.Definition.Pips)
        {
            // ... ������� ������� ������ �������
            _tGameObject = Instantiate(PrefabSprite) as GameObject;
            // ��������� ��������� ������� ������ �����
            _tGameObject.transform.SetParent(card.transform);
            // ���������� localPosititon, ��� ���������� � XML-�����
            _tGameObject.transform.localPosition = pip.Loc;

            // �����������, ���� ����������
            if (pip.Flip)
            {
                _tGameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            // ��������������, ���� ���������� (������ ��� ����)
            if (pip.Scale != 1)
            {
                _tGameObject.transform.localScale = Vector3.one * pip.Scale;
            }

            // ���� ��� �������� �������
            _tGameObject.name = "pip";
            // �������� ������ �� ��������� SpriteRenderer
            _tSpriteRenderer = _tGameObject.GetComponent<SpriteRenderer>();
            // ���������� ������ �����
            _tSpriteRenderer.sprite = DictionarySuits[card.Suit];
            // ���������� sortingOrder, ����� ������ ����������� ��� Card_Front
            _tSpriteRenderer.sortingOrder = 1;
            // �������� ������� ������ � ������ �������
            card.PipGOs.Add(_tGameObject);
        }
    }

    private void AddFace(Card card)
    {
        if (card.Definition.Face == "")
        {
            return; // �����, ���� ��� �� ����� � ���������
        }

        _tGameObject = Instantiate(PrefabSprite) as GameObject;
        _tSpriteRenderer = _tGameObject.GetComponent<SpriteRenderer>();
        // ������������� ��� � �������� ��� � GetFace()
        _tSprite = GetFace(card.Definition.Face + card.Suit);
        _tSpriteRenderer.sprite = _tSprite;   // ���������� ���� ������ � _tSpriteRenderer
        _tSpriteRenderer.sortingOrder = 1;    // ���������� sortingOrder
        _tGameObject.transform.SetParent(card.transform);
        _tGameObject.transform.localPosition = Vector3.zero;
        _tGameObject.name = "face";
    }

    // ������� ������ � ��������� ��� �����
    private Sprite GetFace(string faceS)
    {
        foreach (Sprite _tSpriteP in FaceSprites)
        {
            // ���� ������ ������ � ��������� ������...
            if (_tSpriteP.name == faceS)
            {
                // ... ������� ���
                return _tSpriteP;
            }
        }

        // ���� ������ �� �������, ������� null
        return null;
    }

    private void AddBack(Card card)
    {
        // �������� �������
        // Card_Back ����� ��������� �� ��������� �� �����
        _tGameObject = Instantiate(PrefabSprite) as GameObject;
        _tSpriteRenderer = _tGameObject.GetComponent<SpriteRenderer>();
        _tSpriteRenderer.sprite = CardBack;
        _tGameObject.transform.SetParent(card.transform);
        _tGameObject.transform.localPosition = Vector3.zero;
        // ������� �������� sortingOrder, ��� � ������ ��������
        _tSpriteRenderer.sortingOrder = 2;
        _tGameObject.name = "back";
        card.Back = _tGameObject;

        // �� ��������� ��������� �����
        card.FaceUp = StartFaceUp; // ������������ �������� FaceUp �����
    }

    // ������������ ����� � Deck.Cards
    static public void Shuffle(ref List<Card> oCards)
    {
        // ������� ��������� ������ ��� �������� ���� � ������������ �������
        List<Card> tCards = new List<Card>();

        int ndx; // ����� ������� ������ ������������ �����
        tCards = new List<Card>(); // ���������������� ��������� ������
        // ���������, ���� �� ����� ���������� ��� ����� � �������� ������
        while (oCards.Count > 0)
        {
            // ������� ��������� ������ �����
            ndx = Random.Range(0, oCards.Count);
            // �������� ��� ����� �� ��������� ������...
            tCards.Add(oCards[ndx]);
            // ... � ������� ����� �� ��������� ������
            oCards.RemoveAt(ndx);
        }

        // �������� �������� ������ ���������
        oCards = tCards;
        // �.�. oCards - ��� ��������-������ (ref), ������������ ��������, ���������� � �����, ���� ���������
    }
}
