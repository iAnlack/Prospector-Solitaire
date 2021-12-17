using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.UI;
using System.Globalization;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset DeckXML;
    public TextAsset LayoutXML;
    public float XOffset = 3;
    public float YOffset = -2.5f;
    public Vector3 LayoutCenter;

    [Header("Set Dynamically")]
    public Deck Deck;
    public Layout Layout;
    public List<CardProspector> DrawPile;
    public Transform LayoutAnchor;
    public CardProspector Target;
    public List<CardProspector> Tableau;
    public List<CardProspector> DiscardPile;

    private void Awake()
    {
        S = this;       // ���������� �������-�������� Prospector
        Culturator();   // ����� �� �������������� ����� � ��������
    }

    private void Start()
    {
        Deck = GetComponent<Deck>();    // �������� ��������� Deck
        Deck.InitDeck(DeckXML.text);    // �������� ��� DeckXML
        Deck.Shuffle(ref Deck.Cards);   // ���������� ������, ������� � �� ������

        // ���� �������� ����� ����������������; ������ �� ������ ����������� ���������
        //Card c;
        //for (int cNum = 0; cNum < Deck.Cards.Count; cNum++)
        //{
        //    c = Deck.Cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        Layout = GetComponent<Layout>();     // �������� ��������� Layout
        Layout.ReadLayout(LayoutXML.text);   // �������� ��� ���������� LayoutXML
        DrawPile = ConvertListCardsToListCardProspectors(Deck.Cards);
        LayoutGame();
    }

    // ������� Draw ������� ���� ����� � ������� DrawPile � ���������� �
    private CardProspector Draw()
    {
        CardProspector cd = DrawPile[0];   // ����� 0-�� ����� CardProspector
        DrawPile.RemoveAt(0);              // ������� �� List<DrawPile>
        return cd;                         // � ������� �
    }

    // LayoutGame() ��������� ����� � ��������� ��������� - "�����"
    private void LayoutGame()
    {
        // ������� ������ ������� ������, ������� ����� ������� ������� ���������
        if (LayoutAnchor == null)
        {
            GameObject tGameObject = new GameObject("_LayoutAnchor");
            // ^ ������� ������ ������� ������ � ������ _LayoutAnchor � ��������
            LayoutAnchor = tGameObject.transform;             // �������� ��� ��������� Transform
            LayoutAnchor.transform.position = LayoutCenter;   // ��������� � �����
        }

        CardProspector cardProspector;
        // ��������� �����
        foreach (SlotDef tSlotDef in Layout.SlotDefs)
        {
            // ^ ��������� ����� ���� ����������� SlotDef � Layout.SlotDefs
            cardProspector = Draw(); // ������� ������ ����� (������) �� ������ DrawPile
            cardProspector.FaceUp = tSlotDef.FaceUp; // ���������� � ������� FaceUp � �����������
                                                     // � ������������ � SlotDef
            cardProspector.transform.parent = LayoutAnchor; // ��������� LayoutAnchor � ���������
            // ��� �������� ������� ����������� ��������: Deck.DeckAnchor, ������� ����� �������
            // ������������ � �������� � ������ _Deck
            cardProspector.transform.localPosition = new Vector3(Layout.Multiplier.x * tSlotDef.X,
                                                                 Layout.Multiplier.y * tSlotDef.Y,
                                                                 -tSlotDef.LayerID);
            // ^ ���������� localPosition ����� � ������������ � ������������ � SlotDef
            cardProspector.LayoutID = tSlotDef.ID;
            cardProspector.SlotDef = tSlotDef;
            // ����� CardProspector � �������� ��������� ����� ��������� CardState.Tableau
            cardProspector.State = eCardState.Tableau;
            cardProspector.SetSortingLayerName(tSlotDef.LayerName); // ��������� ���� ����������

            Tableau.Add(cardProspector); // �������� ����� � ������ Tableau
        }
    }

    private List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> listCD)
    {
        List<CardProspector> listCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in listCD)
        {
            tCP = tCD as CardProspector;
            listCP.Add(tCP);
        }

        return listCP;
    }

    // �����, �������� �������� �������������� ��������� ������������ ��������, ��������� � CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
