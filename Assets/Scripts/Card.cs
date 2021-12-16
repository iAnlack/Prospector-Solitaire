using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string Suit;                 // ����� ����� (C, D, H ��� S)
    public int Rank;                    // ����������� ����� (1-14)
    public Color Color = Color.black;   // ���� �������
    public string ColS = "Black";       // (Black/Red) - ��� �����

    // ���� ������ ������ ��� ������� ������� Decorator
    public List<GameObject> DecoGOs = new List<GameObject>();
    // ���� ������ ������ ��� ������� ������� Pip
    public List<GameObject> PipGOs = new List<GameObject>();

    public GameObject Back;             // ������� ������ ������� �����
    public CardDefinition Definition;   // ����������� �� DeckXML.xml

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

[System.Serializable] // ������������� ����� �������� ��� ������ � ����������
public class Decorator
{
    // ���� ����� ������ ���������� �� DeckXML � ������ ������ �� �����
    public string Type;         // ������, ������������ ����������� �����, ����� type "pip"
    public Vector3 Loc;         // �������������� ������� �� �����
    public float Scale = 1f;    // ������� �������
    public bool Flip = false;   // ������� ���������� ������� �� ���������
}

[System.Serializable]
public class CardDefinition
{
    // ���� ����� ������ ���������� � ����������� �����
    public string Face;                                    // ������, ������������ ������� ������� �����
    public int Rank;                                       // ����������� ����� (1-13)
    public List<Decorator> Pips = new List<Decorator>();   // ������
}
