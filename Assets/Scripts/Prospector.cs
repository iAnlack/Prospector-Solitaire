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
        S = this; // Подготовка объекта-одиночки Prospector
    }

    private void Start()
    {
        Deck = GetComponent<Deck>();   // Получить компонент Deck
        Deck.InitDeck(DeckXML.text);   // Передать ему DeckXML
    }
}
