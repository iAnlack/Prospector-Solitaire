using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ������������ �� ����� ���������� ����������� FloatingScore
public enum eFSState
{
    Idle,
    Pre,
    Active,
    Post
}

// FloatingScore ����� ������������ �� ������ �� ���������� ������� ������������ ������ �����
public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState State = eFSState.Idle;

    [SerializeField] protected int Score = 0;
    public string ScoreString;

    // �������� Score ������������� ��� ����: Score � ScoreString
    public int ScoreSetter
    {
        get
        {
            return Score;
        }
        set
        {
            Score = value;
            ScoreString = Score.ToString("N0");   // �������� "N0" ������� �������� ����� � �����

            // ������� � ��������� �� ����� "C# ������ ����������� �������� ��������", ����� �����
            // �������� ��������, �������������� ������� ToString
            GetComponent<Text>().text = ScoreString;
        }
    }

    public List<Vector2> BezierPoints;          // �����, ������������ ������ �����
    public List<float> FontSizes;               // ����� ������ ����� ��� ��������������� ������
    public float TimeStart = -1f;
    public float TimeDuration = 1f;
    public string EasingCurve = Easing.InOut;   // ������� ����������� �� Utils.cs

    // ������� ������, ��� �������� ����� ������ ����� SendMessage,
    // ����� ���� ��������� FloatingScore �������� ��������
    public GameObject ReportFinishTo = null;

    private RectTransform _rectTransform;
    private Text _text;

    private void Update()
    {
        // ���� ���� ������ ������ �� ������������, ������ �����
        if (State == eFSState.Idle)
        {
            return;
        }

        // ��������� u �� ������ �������� ������� � ����������������� ��������
        // u ���������� �� 0 �� 1 (������)
        float u = (Time.time - TimeStart) / TimeDuration;
        // ������������ ����� Easing �� Utils ��� ������������� �������� u
        float uC = Easing.Ease(u, EasingCurve);
        if (u < 0)
        {
            // ���� u < 0, ������ �� ������ ���������
            State = eFSState.Pre;
            _text.enabled = false;   // ���������� ������ �����
        }
        else
        {
            if (u >= 1)
            {
                // ���� u >= 1, ����������� ��������
                uC = 1; // ���������� uC = 1, ����� �� ����� �� ������� �����
                State = eFSState.Post;
                if (ReportFinishTo != null)
                {
                    // ���� ������� ������ ������, ������������ SendMessage ��� ������ ������ FSCallback
                    // � �������� ��� �������� ���������� � ���������
                    ReportFinishTo.SendMessage("FSCallback", this);
                    // ����� �������� ��������� ���������� gameObject
                    Destroy(gameObject);
                }
                else
                {
                    // ���� �� ������, �� ���������� ������� ���������. ������ �������� ��� � �����
                    State = eFSState.Idle;
                }
            }
            else
            {
                // ���� 0 <= u < 1, ������, ������� ��������� ������� � ��������
                State = eFSState.Active;
                _text.enabled = true;   // �������� ����� �����
            }

            // ������������ ������ ����� ��� ����������� � �������� �����
            Vector2 pos = Utils.Bezier(uC, BezierPoints);
            // ������� ����� RectTransform ����� ������������ ��� ���������������� ��������
            // ����������������� ���������� ������������ ������ ������� ������
            _rectTransform.anchorMin = _rectTransform.anchorMax = pos;
            if (FontSizes != null && FontSizes.Count > 0)
            {
                // ���� ������ FontSizes �������� ��������, �� ��������������� FontSize ����� ������� GUIText
                int size = Mathf.RoundToInt(Utils.Bezier(uC, FontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }

    // ��������� FloatingScore � ��������� ��������
    // �������� ��������, ��� ��� ���������� eTimeS � eTimeD ���������� �������� �� ���������
    public void Init(List<Vector2> ePoints, float eTimeS = 0, float eTimeD = 1)
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.anchoredPosition = Vector2.zero;

        _text = GetComponent<Text>();

        BezierPoints = new List<Vector2>(ePoints);

        if (ePoints.Count == 1)   // ���� ������ ������ ���� �����, �� ������ ������������� � ��
        {
            transform.position = ePoints[0];
            return;
        }

        // ���� eTimeS ����� �������� �� ���������, ��������� ������ �� �������� �������
        if (eTimeS == 0)
        {
            eTimeS = Time.time;
        }

        TimeStart = eTimeS;
        TimeDuration = eTimeD;

        State = eFSState.Pre;   // ���������� ��������� pre - ���������� ������ ��������
    }

    public void FSCallBack(FloatingScore floatingScore)
    {
        // ����� SendMessage ������� ��� �������, ��� ������ �������� ����
        // �� ���������� ���������� FloatingScore
        ScoreSetter += floatingScore.ScoreSetter;
    }
}
