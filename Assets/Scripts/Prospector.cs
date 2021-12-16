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

    [Header("Set Dynamically")]
    public Deck Deck;

    private void Awake()
    {
        S = this;       // ���������� �������-�������� Prospector
        Culturator();   // ����� �� �������������� ����� � ��������
    }

    private void Start()
    {
        Deck = GetComponent<Deck>();   // �������� ��������� Deck
        Deck.InitDeck(DeckXML.text);   // �������� ��� DeckXML
    }

    // �����, �������� �������� �������������� ��������� ������������ ��������, ��������� � CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
