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
        LayoutGame();
    }

    // Функция Draw снимает одну карту с вершины DrawPile и возвращает её
    private CardProspector Draw()
    {
        CardProspector cd = DrawPile[0];   // Снять 0-ую карту CardProspector
        DrawPile.RemoveAt(0);              // Удалить из List<DrawPile>
        return cd;                         // и вернуть её
    }

    // LayoutGame() размещает карты в начальной раскладке - "шахте"
    private void LayoutGame()
    {
        // Создать пустой игровой объект, который будет служить центром раскладки
        if (LayoutAnchor == null)
        {
            GameObject tGameObject = new GameObject("_LayoutAnchor");
            // ^ Создать пустой игровой объект с именем _LayoutAnchor в иерархии
            LayoutAnchor = tGameObject.transform;             // Получить его компонент Transform
            LayoutAnchor.transform.position = LayoutCenter;   // Поместить в центр
        }

        CardProspector cardProspector;
        // Разложить карты
        foreach (SlotDef tSlotDef in Layout.SlotDefs)
        {
            // ^ Выполнить обход всех определений SlotDef в Layout.SlotDefs
            cardProspector = Draw(); // Выбрать первую карту (сверху) из стопки DrawPile
            cardProspector.FaceUp = tSlotDef.FaceUp; // Установить её признак FaceUp в соответсвии
                                                     // с определением в SlotDef
            cardProspector.transform.parent = LayoutAnchor; // Назначить LayoutAnchor её родителем
            // Эта операция заменит предыдущего родителя: Deck.DeckAnchor, который после запуска
            // отображается в иерархии с именем _Deck
            cardProspector.transform.localPosition = new Vector3(Layout.Multiplier.x * tSlotDef.X,
                                                                 Layout.Multiplier.y * tSlotDef.Y,
                                                                 -tSlotDef.LayerID);
            // ^ Установить localPosition карты в соответствии с определением в SlotDef
            cardProspector.LayoutID = tSlotDef.ID;
            cardProspector.SlotDef = tSlotDef;
            // Карты CardProspector в основной раскладке имеют состояние CardState.Tableau
            cardProspector.State = eCardState.Tableau;
            cardProspector.SetSortingLayerName(tSlotDef.LayerName); // Назначить слой сортировки

            Tableau.Add(cardProspector); // Добавить карту в список Tableau
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

    // Метод, решающий проблему преобразования локальных особенностей символов, связанной с CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
