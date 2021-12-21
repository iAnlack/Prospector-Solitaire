using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;
using UnityEngine.UI;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset DeckXML;
    public TextAsset LayoutXML;
    public float XOffset = 3;
    public float YOffset = -2.5f;
    public Vector3 LayoutCenter;
    public Vector2 FsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 FsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 FsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 FsPosEnd = new Vector2(0.5f, 0.95f);
    public float ReloadDelay = 2f;   // �������� ����� ��������
    public Text GameOverText, RoundResultText, HighScoreText;

    [Header("Set Dynamically")]
    public Deck Deck;
    public Layout Layout;
    public List<CardProspector> DrawPile;
    public Transform LayoutAnchor;
    public CardProspector Target;
    public List<CardProspector> Tableau;
    public List<CardProspector> DiscardPile;
    public FloatingScore FsRun;

    private void Awake()
    {
        S = this;       // ���������� �������-�������� Prospector
        Culturator();   // ����� �� �������������� ����� � ��������
        SetUpUITexts();
    }

    private void Start()
    {
        Scoreboard.S.ScoreSetter = ScoreManager.SCORE;

        Deck = GetComponent<Deck>();    // �������� ��������� Deck
        Deck.InitDeck(DeckXML.text);    // �������� ��� DeckXML
        Deck.Shuffle(ref Deck.Cards);   // ���������� ������, ������� � �� ������

        // ���� �������� ����� ����������������; ������ �� ������ ����������� ���������
        //Card c;
        //for (int cNum = 0; cNum < Deck.Cards.Count; cNum++)
        //{
        //    c = Deck.Cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        Layout = GetComponent<Layout>();     // �������� ��������� Layout
        Layout.ReadLayout(LayoutXML.text);   // �������� ��� ���������� LayoutXML
        DrawPile = ConvertListCardsToListCardProspectors(Deck.Cards);
        LayoutGame();
    }

    private void SetUpUITexts()
    {
        // ��������� ������ HighScore
        GameObject gameObject = GameObject.Find("HighScore");
        if (gameObject != null)
        {
            HighScoreText = gameObject.GetComponent<Text>();
        }

        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        gameObject.GetComponent<Text>().text = hScore;

        // ��������� �������, ������������ � ����� ������
        gameObject = GameObject.Find("GameOver");
        if (gameObject != null)
        {
            GameOverText = gameObject.GetComponent<Text>();
        }

        gameObject = GameObject.Find("RoundResult");
        if (gameObject != null)
        {
            RoundResultText = gameObject.GetComponent<Text>();
        }

        // ������ �������
        ShowResultsUI(false);
    }

    private void ShowResultsUI(bool show)
    {
        GameOverText.gameObject.SetActive(show);
        RoundResultText.gameObject.SetActive(show);
    }

    // ������� Draw ������� ���� ����� � ������� DrawPile � ���������� �
    private CardProspector Draw()
    {
        CardProspector cd = DrawPile[0];   // ����� 0-�� ����� CardProspector
        DrawPile.RemoveAt(0);              // ������� �� List<DrawPile>
        return cd;                         // � ������� �
    }

    // LayoutGame() ��������� ����� � ��������� ��������� - "�����"
    private void LayoutGame()
    {
        // ������� ������ ������� ������, ������� ����� ������� ������� ���������
        if (LayoutAnchor == null)
        {
            GameObject tGameObject = new GameObject("_LayoutAnchor");
            // ^ ������� ������ ������� ������ � ������ _LayoutAnchor � ��������
            LayoutAnchor = tGameObject.transform;             // �������� ��� ��������� Transform
            LayoutAnchor.transform.position = LayoutCenter;   // ��������� � �����
        }

        CardProspector cardProspector;
        // ��������� �����
        foreach (SlotDef tSlotDef in Layout.SlotDefs)
        {
            // ^ ��������� ����� ���� ����������� SlotDef � Layout.SlotDefs
            cardProspector = Draw(); // ������� ������ ����� (������) �� ������ DrawPile
            cardProspector.FaceUp = tSlotDef.FaceUp; // ���������� � ������� FaceUp � �����������
                                                     // � ������������ � SlotDef
            cardProspector.transform.parent = LayoutAnchor; // ��������� LayoutAnchor � ���������
            // ��� �������� ������� ����������� ��������: Deck.DeckAnchor, ������� ����� �������
            // ������������ � �������� � ������ _Deck
            cardProspector.transform.localPosition = new Vector3(Layout.Multiplier.x * tSlotDef.X,
                                                                 Layout.Multiplier.y * tSlotDef.Y,
                                                                 -tSlotDef.LayerID);
            // ^ ���������� localPosition ����� � ������������ � ������������ � SlotDef
            cardProspector.LayoutID = tSlotDef.ID;
            cardProspector.SlotDef = tSlotDef;
            // ����� CardProspector � �������� ��������� ����� ��������� CardState.Tableau
            cardProspector.State = eCardState.Tableau;
            cardProspector.SetSortingLayerName(tSlotDef.LayerName); // ��������� ���� ����������

            Tableau.Add(cardProspector); // �������� ����� � ������ Tableau
        }

        // ��������� ������ ����, �������� ����������� ������ �����
        foreach (CardProspector tCardProspector in Tableau)
        {
            foreach (int hid in tCardProspector.SlotDef.HiddenBy)
            {
                cardProspector = FindCardByLayoutID(hid);
                tCardProspector.HiddenBy.Add(cardProspector);
            }
        }

        // ������� ��������� ������� �����
        MoveToTarget(Draw());

        // ��������� ������ ��������� ����
        UpdateDrawPile();
    }

    // ����������� ����� ����� layoutID � ��������� CardProspector � ���� �������
    private CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCardProspector in Tableau)
        {
            // ����� �� ���� ������ � ������ Tableau
            if (tCardProspector.LayoutID == layoutID)
            {
                // ���� ����� ����� ����� ��������� � �������, ������� �
                return tCardProspector;
            }
        }

        // ���� ������ �� �������, ������� null
        return null;
    }

    // ������������ ����� � �������� ��������� ������� �������� ����� ��� ����
    private void SetTableauFaces()
    {
        foreach (CardProspector cd in Tableau)
        {
            bool faceUp = true;   // ������������, ��� ����� ������ ���� �������� ������� �������� �����
            foreach (CardProspector cover in cd.HiddenBy)
            {
                // ���� ����� �� ����, ������������� �������, ������������ � �������� ���������
                if (cover.State == eCardState.Tableau)
                {
                    faceUp = false;   // ����������� ������� �������� ����
                }
            }

            cd.FaceUp = faceUp;       // ��������� ����� ��� ��� �����
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

    // ���������� ������� ������� ����� � ������ ���������� ����
    private void MoveToDiscard(CardProspector cd)
    {
        // ���������� ��������� ����� � Discard (��������)
        cd.State = eCardState.Discard;
        DiscardPile.Add(cd);                  // �������� � � ������ DiscardPile
        cd.transform.parent = LayoutAnchor;   // �������� �������� transform.parent

        // ����������� ��� ����� � ������� ������ ���������� ����
        cd.transform.localPosition = new Vector3(Layout.Multiplier.x * Layout.DiscardPile.X,
                                                 Layout.Multiplier.y * Layout.DiscardPile.Y,
                                                 -Layout.DiscardPile.LayerID + 0.5f);
        cd.FaceUp = true;
        // ��������� ������ ������ ��� ���������� �� �������
        cd.SetSortingLayerName(Layout.DiscardPile.LayerName);
        cd.SetSortOrder(-100 + DiscardPile.Count);
    }

    // ������ ����� cd ����� ������� ������
    private void MoveToTarget(CardProspector cd)
    {
        // ���� ������� ����� ����������, ����������� � � ������ ���������� ����
        if (Target != null)
        {
            MoveToDiscard(Target);
        }

        Target = cd;   // cd - ����� ������� �����
        cd.State = eCardState.Target;
        cd.transform.parent = LayoutAnchor;

        // ����������� �� ����� ��� ������� �����
        cd.transform.localPosition = new Vector3(Layout.Multiplier.x * Layout.DiscardPile.X,
                                                 Layout.Multiplier.y * Layout.DiscardPile.Y,
                                                 -Layout.DiscardPile.LayerID);
        cd.FaceUp = true;   // ����������� ������� �������� �����
        // ��������� ���������� �� �������
        cd.SetSortingLayerName(Layout.DiscardPile.LayerName);
        cd.SetSortOrder(0);
    }

    // ������������ ������ ��������� ����, ����� ���� �����, ������� ���� ��������
    private void UpdateDrawPile()
    {
        CardProspector cd;
        // ��������� ����� ���� ���� � DrawPile
        for (int i = 0; i < DrawPile.Count; i++)
        {
            cd = DrawPile[i];
            cd.transform.parent = LayoutAnchor;

            // ����������� � ������ �������� Layout.DrawPile.Stagger
            Vector2 dpStagger = Layout.DrawPile.Stagger;
            cd.transform.localPosition = new Vector3(Layout.Multiplier.x * (Layout.DrawPile.X + i * dpStagger.x),
                                                     Layout.Multiplier.y * (Layout.DrawPile.Y + i * dpStagger.y),
                                                     -Layout.DrawPile.LayerID + 0.1f * i);
            cd.FaceUp = false;   // ��������� ������� �������� ����
            cd.State = eCardState.DrawPile;
            // ��������� ���������� �� �������
            cd.SetSortingLayerName(Layout.DrawPile.LayerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    // CardClicked ���������� � ����� �� ������ �� ����� �����
    public void CardClicked(CardProspector cd)
    {
        // ������� ������������ ���������� �����
        switch (cd.State)
        {
            case eCardState.DrawPile:
                // ������ �� ����� ����� � ������ ��������� ���� �������� � ����� ������� �����
                MoveToDiscard(Target);   // ����������� ������� ����� � DiscardPile
                MoveToTarget(Draw());    // ����������� ������ ��������� ����� �� ����� �������
                UpdateDrawPile();        // �������� ��������� ������ ��������� ����
                ScoreManager.EVENT(eScoreEvent.Draw);
                FloatingScoreHandler(eScoreEvent.Draw);
                break;

            case eCardState.Tableau:
                // ��� ����� � �������� ��������� ����������� ����������� � ����������� �� ����� �������
                bool validMatch = true;
                if (!cd.FaceUp)
                {
                    // �����, ��������� ������� �������� ����, �� ����� ������������
                    validMatch = false;
                }
                if (!AdjacentRank(cd, Target))
                {
                    // ���� ������� ����������� �� �����������, ����� �� ����� ������������
                    validMatch = false;
                }
                if (!validMatch)
                {
                    return;   // �����, ���� ����� �� ����� ������������
                }

                // �� ��������� �����: ���! ����� ����� �����������.
                Tableau.Remove(cd);   // ������� �� ������ Tableau
                MoveToTarget(cd);     // ������� ��� ����� �������
                SetTableauFaces();    // ��������� ����� � �������� ��������� ������� �������� ���� ��� �����
                ScoreManager.EVENT(eScoreEvent.Mine);
                FloatingScoreHandler(eScoreEvent.Mine);
                break;

            case eCardState.Target:
                // ������ �� ������� ����� ������������
                break;
        }

        // ��������� ���������� ����
        CheckForGameOver();
    }

    // ��������� ���������� ����
    private void CheckForGameOver()
    {
        // ���� �������� ��������� ��������, ���� ���������
        if (Tableau.Count == 0)
        {
            // ������� GameOver() � ��������� ������
            GameOver(true);
            return;
        }

        // ���� ��� ���� ��������� �����, ���� �� �����������
        if (DrawPile.Count > 0)
        {
            return;
        }

        // ��������� ������� ���������� �����
        foreach (CardProspector cd in Tableau)
        {
            if (AdjacentRank(cd, Target))
            {
                // ���� ���� ���������� ���, ���� �� �����������
                return;
            }
        }

        // �.�. ���������� ����� ���, ���� �����������
        // ������� GameOver() � ��������� ���������
        GameOver(false);
    }

    // ����������, ����� ���� �����������.
    private void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (FsRun != null)
        {
            score += FsRun.ScoreSetter;
        }

        if (won)
        {
            GameOverText.text = "Round Over";
            RoundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            //Debug.Log("Game Over. You won! :)");
            ScoreManager.EVENT(eScoreEvent.GameWin);
            FloatingScoreHandler(eScoreEvent.GameWin);
        }
        else
        {
            GameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                RoundResultText.text = str;
            }
            else
            {
                RoundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            //Debug.Log("Game Over. You Lost. :(");
            ScoreManager.EVENT(eScoreEvent.GameLoss);
            FloatingScoreHandler(eScoreEvent.GameLoss);
        }

        // ������������� ����� � �������� ���� � �������� ���������
        //SceneManager.LoadScene("Prospector Scene 0");

        // ������������� ����� ����� ReloadDelay ������
        // ��� �������� ����� � ������ �������� �� ����� ����������
        Invoke("ReloadLevel", ReloadDelay);
    }

    private void ReloadLevel()
    {
        // ������������� ����� � �������� ���� � �������� ���������
        SceneManager.LoadScene("Prospector Scene 0");
    }

    // ���������� true, ���� ��� ����� ������������� ������� �����������
    // (� ������ ������������ �������� ����������� ����� ����� � ������)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // ���� ����� �� ���� �������� ������� �������� ����, ������� ����������� �� �����������
        if (!c0.FaceUp || !c1.FaceUp)
        {
            return false;
        }

        // ���� ����������� ���� ���������� �� 1, ������� ����������� �����������
        if (Mathf.Abs(c0.Rank - c1.Rank) == 1)
        {
            return true;
        }

        // ���� ���� ����� - ���, � ������ ������, ������� ����������� �����������
        if (c0.Rank == 1 && c1.Rank == 13)
        {
            return true;
        }
        if (c0.Rank == 13 && c1.Rank == 1)
        {
            return true;
        }

        // ����� ������� false
        return false;
    }

    // ������������ �������� FloatingScore
    private void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPoints;
        switch (evt)
        {
            // � ������ ������, ��������� � ���������� ���� ����������� ���� � �� �� ��������
            case eScoreEvent.Draw:       // ����� ��������� �����
            case eScoreEvent.GameWin:    // ������ � ������
            case eScoreEvent.GameLoss:   // ��������� � ������
                // �������� FsRun � Scoreboard
                if (FsRun != null)
                {
                    // ������� ����� ��� ������ �����
                    fsPoints = new List<Vector2>();
                    fsPoints.Add(FsPosRun);
                    fsPoints.Add(FsPosMid2);
                    fsPoints.Add(FsPosEnd);
                    FsRun.ReportFinishTo = Scoreboard.S.gameObject;
                    FsRun.Init(fsPoints, 0, 1);
                    // ����� ��������������� FontSize
                    FsRun.FontSizes = new List<float>(new float[] { 28, 36, 4 });
                    FsRun = null;   // �������� FsRun, ����� ������� ������
                }
                break;

            case eScoreEvent.Mine:       // �������� ����� �� �������� ���������
                // ������� FloatingScore ��� ����������� ����� ���������� �����
                FloatingScore floatingScore;
                // ����������� �� ������� ��������� ���� mousePosition � FsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPoints = new List<Vector2>();
                fsPoints.Add(p0);
                fsPoints.Add(FsPosMid);
                fsPoints.Add(FsPosRun);
                floatingScore = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPoints);
                floatingScore.FontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (FsRun == null)
                {
                    FsRun = floatingScore;
                    FsRun.ReportFinishTo = null;
                }
                else
                {
                    floatingScore.ReportFinishTo = FsRun.gameObject;
                }
                break;
        }
    }


    // �����, �������� �������� �������������� ��������� ������������ ��������, ��������� � CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
