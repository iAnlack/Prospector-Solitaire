using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;   // ������-�������� Scoreboard

    [Header("Set in Inspector")]
    public GameObject PrefabFloatingScore;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform _canvasTransform;

    // �������� ScoreProperty ����� ������������� ScoreString
    public int ScoreSetter
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            ScoreStringSetter = _score.ToString("N0");
        }
    }

    // �������� ScoreText ����� ������������� Text.text
    public string ScoreStringSetter
    {
        get
        {
            return _scoreString;
        }
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Awake()
    {
        if (S == null)
        {
            S = this;   // ���������� �������� �������-��������
        }
        else
        {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }

        _canvasTransform = transform.parent;
    }

    // ����� ���������� ������� SendMessage, ���������� floatingScore � this.Score
    public void FSCallback(FloatingScore floatingScore)
    {
        ScoreSetter += floatingScore.ScoreSetter;
    }

    // ������ � �������������� ����� ������� ������ FloatingScore.
    // ���������� ��������� �� ��������� ��������� FloatingScore, ����� ���������� ������� �����
    // ��������� � ��� �������������� �������� (��������, ���������� ������ FontSizes � �.�.)
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> points)
    {
        GameObject gameObject = Instantiate<GameObject>(PrefabFloatingScore);
        gameObject.transform.SetParent(_canvasTransform);
        FloatingScore floatingScore = gameObject.GetComponent<FloatingScore>();
        floatingScore.ScoreSetter = amt;
        floatingScore.ReportFinishTo = this.gameObject;   // ��������� �������� �����
        floatingScore.Init(points);
        return floatingScore;
    }
}
