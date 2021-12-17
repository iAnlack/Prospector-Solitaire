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

    [Header("Set Dynamically")]
    public Deck Deck;
    public Layout Layout;

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
    }

    // �����, �������� �������� �������������� ��������� ������������ ��������, ��������� � CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
