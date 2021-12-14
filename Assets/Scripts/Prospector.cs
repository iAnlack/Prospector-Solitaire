using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.UI;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset DeckXML;

    [Header("Set Dynamically")]
    public Deck Deck;

    private void Awake()
    {
        S = this; // ���������� �������-�������� Prospector
    }

    private void Start()
    {
        Deck = GetComponent<Deck>();   // �������� ��������� Deck
        Deck.InitDeck(DeckXML.text);   // �������� ��� DeckXML
    }
}
