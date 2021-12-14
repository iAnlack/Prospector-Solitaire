using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    
}

[System.Serializable] // ������������� ����� �������� ��� ������ � ����������
public class Decorator
{
    // ���� ����� ������ ���������� �� DeckXML � ������ ������ �� �����
    public Vector3 Loc;         // �������������� ������� �� �����
    public float Scale = 1f;    // ������� �������
    public string Type;         // ������, ������������ ����������� �����, ����� type "pip"
    public bool Flip = false;   // ������� ���������� ������� �� ���������
}

[System.Serializable]
public class CardDefinition
{
    // ���� ����� ������ ���������� � ����������� �����
    public int Rank;                                       // ����������� ����� (1-13)
    public string Face;                                    // ������, ������������ ������� ������� �����
    public List<Decorator> Pips = new List<Decorator>();   // ������
}
