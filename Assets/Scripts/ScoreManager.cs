using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������������ �� ����� ���������� ��������� ���������� �����
public enum eScoreEvent
{
    Draw,
    Mine,
    MineGold,
    GameWin,
    GameLoss
}

// ScoreManager ��������� ��������� �����
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dinamically")]
    // ���� ��� �������� ���������� � ������������ �����
    public int Chain = 0;
    public int ScoreRun = 0;
    public int Score = 0;

    private void Awake()
    {
        if (S == null)
        {
            S = this;   // ���������� �������� ������� ��������
        }
        else
        {
            Debug.LogError("ERROR: ScoreManager.Awake(): S is already set!");
        }

        // ��������� ������ � PlayerPrefs
        if (PlayerPrefs.HasKey ("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        // �������� ����, ������������ � ��������� ������, ������� ������ ���� > 0, ���� ����� ���������� �������
        Score += SCORE_FROM_PREV_ROUND;
        // � �������� SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(eScoreEvent evt)
    {
        try
        {
            // try-catch �� �������� ������ �������� ��������� ���������
            S.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {

            Debug.LogError("ScoreManager: EVENT() called while S = null.\n" + nre);
        }
    }

    private void Event(eScoreEvent evt)
    {
        switch (evt)
        {
            // � ������ ������, ��������� � ���������� ���� ����������� ���� � �� �� ��������
            case eScoreEvent.Draw:       // ����� ��������� �����
            case eScoreEvent.GameWin:    // ������ � ������
            case eScoreEvent.GameLoss:   // ��������� � ������
                Chain = 0;               // �������� ������� �������� �����
                Score += ScoreRun;       // �������� ScoreRun � ������ ����� �����
                ScoreRun = 0;            // �������� ScoreRun
                break;

            case eScoreEvent.Mine:       // �������� ����� �� �������� ���������
                Chain++;                 // ��������� ���������� ����� � �������
                ScoreRun += Chain;       // �������� ���� � �����
                break;
        }

        // ��� ������ ���������� switch ������������ ������ � �������� � ������
        switch (evt)
        {
            case eScoreEvent.GameWin:
                // � ������ ������ ��������� ���� � ��������� ����� �������������� ���� �� ������������ �������
                // SceneManager.LoadScene()
                SCORE_FROM_PREV_ROUND = Score;
                Debug.Log("You won this round! Round score: " + Score);
                break;

            case eScoreEvent.GameLoss:
                // � ������ ��������� �������� � ��������
                if (HIGH_SCORE <= Score)
                {
                    Debug.Log("You got the high score! High score: " + Score);
                    HIGH_SCORE = Score;
                    PlayerPrefs.SetInt("ProsprectorHighScore", Score);
                }
                else
                {
                    Debug.Log("Your final score for the game was: " + Score);
                }
                break;

            default:
                Debug.Log("Score: " + Score + " ScoreRun:" + ScoreRun + " Chain:" + Chain);
                break;
        }
    }

    static public int CHAIN
    {
        get
        {
            return S.Chain;
        }
    }

    static public int SCORE
    {
        get
        {
            return S.Score;
        }
    }

    static public int SCORE_RUN
    {
        get
        {
            return S.ScoreRun;
        }
    }
}
