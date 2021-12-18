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
    // ������ ����������� SpriteRenderer ����� � ��������� � ���� ������� ��������
    public SpriteRenderer[] SpriteRenderers;

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

    private void Start()
    {
        SetSortOrder(0);   // ��������� ���������� ���������� ����
    }

    // ���� SpriteRenderers �� ��������, ��� ������� ��������� ���
    public void PopulateSpriteRenderers()
    {
        // ���� SpriteRenderers �������� null ��� ������ ������
        if (SpriteRenderers == null || SpriteRenderers.Length == 0)
        {
            // �������� ���������� SpriteRenderer ����� �������� ������� � ��������� � ���� ������� ��������
            SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    // �������������� ���� sortingLayerName �� ���� ����������� SpriteRenderer
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer tSpriteRenderer in SpriteRenderers)
        {
            tSpriteRenderer.sortingLayerName = tSLN;
        }
    }

    // �������������� ���� sortingOrder ���� ����������� SpriteRenderer
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        // ��������� ����� ���� ��������� � ������ SpriteRenderers
        foreach (SpriteRenderer tSpriteRenderer in SpriteRenderers)
        {
            if (tSpriteRenderer.gameObject == this.gameObject)
            {
                // ���� ��������� ����������� � �������� �������� �������, ��� ���
                tSpriteRenderer.sortingOrder = sOrd; // ���������� ���������� ����� ��� ���������� � sOrd
                continue;                            // � ������� � ��������� �������� �����
            }

            // ������ �������� ������� ������ ����� ���
            // ���������� ���������� ����� ��� ����������, � ����������� �� �����
            switch (tSpriteRenderer.gameObject.name)
            {
                case "back":
                    // ���������� ���������� ���������� ����� ��� ����������� ������ ������ ��������
                    tSpriteRenderer.sortingOrder = sOrd + 2;
                    break;

                case "face": // ���� ��� "face"
                default:     // ��� �� ������
                    // ���������� ������������� ���������� ����� ��� ����������� ������ ����
                    tSpriteRenderer.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    // ����������� ������ ����� ���������������� � ���������� ������������ ������� � ���� �� �������
    virtual public void OnMouseUpAsButton()
    {
        Debug.Log(name);   // �� ������ ��� ������ ������� ��� �����
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
