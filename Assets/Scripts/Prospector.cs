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
    public List<CardProspector> DrawPile;

    private void Awake()
    {
        S = this;       // Подготовка объекта-одиночки Prospector
        Culturator();   // Метод по преобразованию говна в конфетку
    }

    private void Start()
    {
        Deck = GetComponent<Deck>();    // Получить компонент Deck
        Deck.InitDeck(DeckXML.text);    // Передать ему DeckXML
        Deck.Shuffle(ref Deck.Cards);   // Перемешать колоду, передав её по ссылке

        // Этот фрагмент можно закомментировать; сейчас мы создаём фактическую раскладку
        //Card c;
        //for (int cNum = 0; cNum < Deck.Cards.Count; cNum++)
        //{
        //    c = Deck.Cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        Layout = GetComponent<Layout>();     // Получить компонент Layout
        Layout.ReadLayout(LayoutXML.text);   // Передать ему содержимое LayoutXML
        DrawPile = ConvertListCardsToListCardProspectors(Deck.Cards);
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> listCD)
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

    // Метод, решающий проблему преобразования локальных особенностей символов, связанной с CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
