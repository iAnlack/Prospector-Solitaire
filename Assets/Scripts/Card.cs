using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string Suit;                 // Масть карты (C, D, H или S)
    public int Rank;                    // Достоинство карты (1-14)
    public Color Color = Color.black;   // Цвет значков
    public string ColS = "Black";       // (Black/Red) - имя цвета

    // Этот список хранит все игровые объекты Decorator
    public List<GameObject> DecoGOs = new List<GameObject>();
    // Этот список хранит все игровые объекты Pip
    public List<GameObject> PipGOs = new List<GameObject>();

    public GameObject Back;             // Игровой объект рубашки карты
    public CardDefinition Definition;   // Извлекается из DeckXML.xml

    public bool FaceUp
    {
        get
        {
            return (!Back.activeSelf);
        }
        set
        {
            Back.SetActive(!value);
        }
    }
}

[System.Serializable] // Сериализуемый класс доступен для правки в инспекторе
public class Decorator
{
    // Этот класс хранит информацию из DeckXML о каждом значке на карте
    public string Type;         // Значок, определяющий достоинство карты, имеет type "pip"
    public Vector3 Loc;         // Местоположение спрайта на карте
    public float Scale = 1f;    // Масштаб спрайта
    public bool Flip = false;   // Признак переворода спрайта по вертикали
}

[System.Serializable]
public class CardDefinition
{
    // Этот класс хранит информацию о достоинстве карты
    public string Face;                                    // Спрайт, изображающий лицевую сторону карты
    public int Rank;                                       // Достоинство карты (1-13)
    public List<Decorator> Pips = new List<Decorator>();   // Значки
}
