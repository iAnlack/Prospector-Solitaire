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

    // InitDeck вызывается экземпляром Prospector, когда будет готов
    public void InitDeck(string deckXMLText)
    {
        ReadDeck(deckXMLText);
    }

    // ReadDeck читает указанный XML-файл и создаёт массив экземпляров CardDefinition
    public void ReadDeck(string deckXMLText)
    {
        XMLR = new PT_XMLReader();   // Создать новый экземпляр PT_XMLReader
        XMLR.Parse(deckXMLText);     // Использовать его для  чтения DeckXML

        // Вывод проверочной строки, чтобы показать, как использовать XMLR.
        // За дополнительной информацией о чтении файлов XML обращайтесь к приложению "Полезные идеи"
        string s = "xml[0] decorator[0] ";
        s += "type=" + XMLR.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + XMLR.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + XMLR.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + XMLR.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        // Прочитать элементы <Decorator> для всех карт
        Decorators = new List<Decorator>(); // Инициализировать список экземпляров Decorator
        // Извлечь список PT_XMLHashList всех элементов <Decorator> из XML-файла
        PT_XMLHashList xDecos = XMLR.xml["xml"][0]["decorator"];
        Decorator decorator;
        for (int i = 0; i < xDecos.Count; i++)
        {
            // Для каждого элемента <Decorator> в XML
            decorator = new Decorator(); // Создать экземпляр Decorator
            // Скопировать атрибуты из <Decorator> в Decorator
            decorator.Type = xDecos[i].att("type");
            // decorator.Flip получит значение true, если аттрибут flip содержит текст "1"
            decorator.Flip = (xDecos[i].att("flip") == "1");
            // Получить значения float из строковых атрибутов
            decorator.Scale = float.Parse(xDecos[i].att("scale"));
            // Vector3 Loc инициализируется как [0, 0, 0], поэтому нам остаётся только изменить его
            decorator.Loc.x = float.Parse(xDecos[i].att("x"));
            decorator.Loc.y = float.Parse(xDecos[i].att("y"));
            decorator.Loc.z = float.Parse(xDecos[i].att("z"));
            // Добавить decorator в список Decorators
            Decorators.Add(decorator);
        }

        // Прочитать координаты для значков, определяющих достоинство карты
        CardDefinitions = new List<CardDefinition>(); // Инициализировать список карт
        // Извлечь список PT_XMLHashList всех элементов <Card> из XML-файла
        PT_XMLHashList xCardDefinitions = XMLR.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefinitions.Count; i++)
        {
            // Для каждого элемента <Card> создать экземпляр CardDefinition
            CardDefinition cDef = new CardDefinition();
            // Получить значения атрибута и добавить их в cDef
            cDef.Rank = int.Parse(xCardDefinitions[i].att("rank"));
            // Извлечь список PT_XMLHashList всех элементов <pip> внутри этого элемента <Card>
            PT_XMLHashList xPips = xCardDefinitions[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    // Обойти все элементы <pip>
                    decorator = new Decorator();
                    // Элементы <pip> в <Card> обрабатываются классом Decorator
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

            // Карты с картинками (Валет, Дама, Король) имеют атрибут face
            if (xCardDefinitions[i].HasAtt("face"))
            {
                cDef.Face = xCardDefinitions[i].att("face");
            }
            CardDefinitions.Add(cDef);
        }
    }
}
