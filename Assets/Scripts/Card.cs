using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    
}

[System.Serializable] // Сериализуемый класс доступен для правки в инспекторе
public class Decorator
{
    // Этот класс хранит информацию из DeckXML о каждом значке на карте
    public Vector3 Loc;         // Местоположение спрайта на карте
    public float Scale = 1f;    // Масштаб спрайта
    public string Type;         // Значок, определяющий достоинство карты, имеет type "pip"
    public bool Flip = false;   // Признак переворода спрайта по вертикали
}

[System.Serializable]
public class CardDefinition
{
    // Этот класс хранит информацию о достоинстве карты
    public int Rank;                                       // Достоинство карты (1-13)
    public string Face;                                    // Спрайт, изображающий лицевую сторону карты
    public List<Decorator> Pips = new List<Decorator>();   // Значки
}
