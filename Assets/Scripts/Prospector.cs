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
    public float ReloadDelay = 2f;   // Задержка между раундами
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
        S = this;       // Подготовка объекта-одиночки Prospector
        Culturator();   // Метод по преобразованию говна в конфетку
        SetUpUITexts();
    }

    private void Start()
    {
        Scoreboard.S.ScoreSetter = ScoreManager.SCORE;

        Deck = GetComponent<Deck>();    // Получить компонент Deck
        Deck.InitDeck(DeckXML.text);    // Передать ему DeckXML
        Deck.Shuffle(ref Deck.Cards);   // Перемешать колоду, передав её по ссылке

        // Этот фрагмент можно закомментировать; сейчас мы создаём фактическую раскладку
        //Card c;
        //for (int cNum = 0; cNum < Deck.Cards.Count; cNum++)
        //{
        //    c = Deck.Cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        Layout = GetComponent<Layout>();     // Получить компонент Layout
        Layout.ReadLayout(LayoutXML.text);   // Передать ему содержимое LayoutXML
        DrawPile = ConvertListCardsToListCardProspectors(Deck.Cards);
        LayoutGame();
    }

    private void SetUpUITexts()
    {
        // Настроить объект HighScore
        GameObject gameObject = GameObject.Find("HighScore");
        if (gameObject != null)
        {
            HighScoreText = gameObject.GetComponent<Text>();
        }

        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        gameObject.GetComponent<Text>().text = hScore;

        // Настроить надписи, отображаемые в конце раунда
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

        // Скрыть надписи
        ShowResultsUI(false);
    }

    private void ShowResultsUI(bool show)
    {
        GameOverText.gameObject.SetActive(show);
        RoundResultText.gameObject.SetActive(show);
    }

    // Функция Draw снимает одну карту с вершины DrawPile и возвращает её
    private CardProspector Draw()
    {
        CardProspector cd = DrawPile[0];   // Снять 0-ую карту CardProspector
        DrawPile.RemoveAt(0);              // Удалить из List<DrawPile>
        return cd;                         // и вернуть её
    }

    // LayoutGame() размещает карты в начальной раскладке - "шахте"
    private void LayoutGame()
    {
        // Создать пустой игровой объект, который будет служить центром раскладки
        if (LayoutAnchor == null)
        {
            GameObject tGameObject = new GameObject("_LayoutAnchor");
            // ^ Создать пустой игровой объект с именем _LayoutAnchor в иерархии
            LayoutAnchor = tGameObject.transform;             // Получить его компонент Transform
            LayoutAnchor.transform.position = LayoutCenter;   // Поместить в центр
        }

        CardProspector cardProspector;
        // Разложить карты
        foreach (SlotDef tSlotDef in Layout.SlotDefs)
        {
            // ^ Выполнить обход всех определений SlotDef в Layout.SlotDefs
            cardProspector = Draw(); // Выбрать первую карту (сверху) из стопки DrawPile
            cardProspector.FaceUp = tSlotDef.FaceUp; // Установить её признак FaceUp в соответсвии
                                                     // с определением в SlotDef
            cardProspector.transform.parent = LayoutAnchor; // Назначить LayoutAnchor её родителем
            // Эта операция заменит предыдущего родителя: Deck.DeckAnchor, который после запуска
            // отображается в иерархии с именем _Deck
            cardProspector.transform.localPosition = new Vector3(Layout.Multiplier.x * tSlotDef.X,
                                                                 Layout.Multiplier.y * tSlotDef.Y,
                                                                 -tSlotDef.LayerID);
            // ^ Установить localPosition карты в соответствии с определением в SlotDef
            cardProspector.LayoutID = tSlotDef.ID;
            cardProspector.SlotDef = tSlotDef;
            // Карты CardProspector в основной раскладке имеют состояние CardState.Tableau
            cardProspector.State = eCardState.Tableau;
            cardProspector.SetSortingLayerName(tSlotDef.LayerName); // Назначить слой сортировки

            Tableau.Add(cardProspector); // Добавить карту в список Tableau
        }

        // Настроить списки карт, мешающих перевернуть данную карту
        foreach (CardProspector tCardProspector in Tableau)
        {
            foreach (int hid in tCardProspector.SlotDef.HiddenBy)
            {
                cardProspector = FindCardByLayoutID(hid);
                tCardProspector.HiddenBy.Add(cardProspector);
            }
        }

        // Выбрать начальную целевую карту
        MoveToTarget(Draw());

        // Разложить стопку свободных карт
        UpdateDrawPile();
    }

    // Преобразует номер слота layoutID в экземпляр CardProspector с этим номером
    private CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCardProspector in Tableau)
        {
            // Поиск по всем картам в списке Tableau
            if (tCardProspector.LayoutID == layoutID)
            {
                // Если номер слота карты совпадает с искомым, вернуть её
                return tCardProspector;
            }
        }

        // Если ничего не найдено, вернуть null
        return null;
    }

    // Поворачивает карты в основной раскладке лицевой стороной вверх или вниз
    private void SetTableauFaces()
    {
        foreach (CardProspector cd in Tableau)
        {
            bool faceUp = true;   // Предположить, что карта должна быть повёрнута лицевой стороной вверх
            foreach (CardProspector cover in cd.HiddenBy)
            {
                // Если любая из карт, перекрывающих текущую, присутствует в основной раскладке
                if (cover.State == eCardState.Tableau)
                {
                    faceUp = false;   // Перевернуть лицевой стороной вниз
                }
            }

            cd.FaceUp = faceUp;       // Повернуть карту так или иначе
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

    // Перемещает текущую целевую карту в стопку сброшенных карт
    private void MoveToDiscard(CardProspector cd)
    {
        // Установить состояние карты в Discard (сброшена)
        cd.State = eCardState.Discard;
        DiscardPile.Add(cd);                  // Добавить её в список DiscardPile
        cd.transform.parent = LayoutAnchor;   // Обновить значение transform.parent

        // Переместить эту карту в позицию стопки сброшенных карт
        cd.transform.localPosition = new Vector3(Layout.Multiplier.x * Layout.DiscardPile.X,
                                                 Layout.Multiplier.y * Layout.DiscardPile.Y,
                                                 -Layout.DiscardPile.LayerID + 0.5f);
        cd.FaceUp = true;
        // Поместить поверх стопки для сортировки по глубине
        cd.SetSortingLayerName(Layout.DiscardPile.LayerName);
        cd.SetSortOrder(-100 + DiscardPile.Count);
    }

    // Делает карту cd новой целевой картой
    private void MoveToTarget(CardProspector cd)
    {
        // Если целевая карта существует, переместить её в стопку сброшенных карт
        if (Target != null)
        {
            MoveToDiscard(Target);
        }

        Target = cd;   // cd - новая целевая карта
        cd.State = eCardState.Target;
        cd.transform.parent = LayoutAnchor;

        // Переместить на место для целевой карты
        cd.transform.localPosition = new Vector3(Layout.Multiplier.x * Layout.DiscardPile.X,
                                                 Layout.Multiplier.y * Layout.DiscardPile.Y,
                                                 -Layout.DiscardPile.LayerID);
        cd.FaceUp = true;   // Перевернуть лицевой стороной вверх
        // Настроить сортировку по глубине
        cd.SetSortingLayerName(Layout.DiscardPile.LayerName);
        cd.SetSortOrder(0);
    }

    // Раскладывает стопку свободных карт, чтобы было видно, сколько карт осталось
    private void UpdateDrawPile()
    {
        CardProspector cd;
        // Выполнить обход всех карт в DrawPile
        for (int i = 0; i < DrawPile.Count; i++)
        {
            cd = DrawPile[i];
            cd.transform.parent = LayoutAnchor;

            // Расположить с учётом смещения Layout.DrawPile.Stagger
            Vector2 dpStagger = Layout.DrawPile.Stagger;
            cd.transform.localPosition = new Vector3(Layout.Multiplier.x * (Layout.DrawPile.X + i * dpStagger.x),
                                                     Layout.Multiplier.y * (Layout.DrawPile.Y + i * dpStagger.y),
                                                     -Layout.DrawPile.LayerID + 0.1f * i);
            cd.FaceUp = false;   // Повернуть лицевой стороной вниз
            cd.State = eCardState.DrawPile;
            // Настроить сортировку по глубине
            cd.SetSortingLayerName(Layout.DrawPile.LayerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    // CardClicked вызывается в ответ на щелчок на любой карте
    public void CardClicked(CardProspector cd)
    {
        // Реакция определяется состоянием карты
        switch (cd.State)
        {
            case eCardState.DrawPile:
                // Щелчок на любой карте в стопке свободных карт приводит к смене целевой карты
                MoveToDiscard(Target);   // Переместить целевую карту в DiscardPile
                MoveToTarget(Draw());    // Переместить верхюю свободную карту на место целевой
                UpdateDrawPile();        // Повторно разложить стопку свободных карт
                ScoreManager.EVENT(eScoreEvent.Draw);
                FloatingScoreHandler(eScoreEvent.Draw);
                break;

            case eCardState.Tableau:
                // Для карты в основной раскладке проверяется возможность её перемещения на место целевой
                bool validMatch = true;
                if (!cd.FaceUp)
                {
                    // Карта, повёрнутая лицевой стороной вниз, не может перемещаться
                    validMatch = false;
                }
                if (!AdjacentRank(cd, Target))
                {
                    // Если правило старшинства не соблюдается, карта не может перемещаться
                    validMatch = false;
                }
                if (!validMatch)
                {
                    return;   // Выйти, если карта на может перемещаться
                }

                // Мы оказались здесь: УРА! Карту можно переместить.
                Tableau.Remove(cd);   // Удалить из списка Tableau
                MoveToTarget(cd);     // Сделать эту карту целевой
                SetTableauFaces();    // Повернуть карты в основной раскладке лицевой стороной вниз или вверх
                ScoreManager.EVENT(eScoreEvent.Mine);
                FloatingScoreHandler(eScoreEvent.Mine);
                break;

            case eCardState.Target:
                // Щелчок на целевой карте игнорируется
                break;
        }

        // Проверить завершение игры
        CheckForGameOver();
    }

    // Проверяет завершение игры
    private void CheckForGameOver()
    {
        // Если основная раскладка опустела, игра завершена
        if (Tableau.Count == 0)
        {
            // Вызвать GameOver() с признаком победы
            GameOver(true);
            return;
        }

        // Если ещё есть свободные карты, игра не завершилась
        if (DrawPile.Count > 0)
        {
            return;
        }

        // Проверить наличие допустимых ходов
        foreach (CardProspector cd in Tableau)
        {
            if (AdjacentRank(cd, Target))
            {
                // Если есть допустимый ход, игра не завершилась
                return;
            }
        }

        // Т.к. допустимых ходов нет, игра завершилась
        // Вызвать GameOver() с признаком проигрыша
        GameOver(false);
    }

    // Вызывается, когда игра завершилась.
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

        // Перезагрузить сцену и сбросить игру в исходное состояние
        //SceneManager.LoadScene("Prospector Scene 0");

        // Перезагрузить сцену через ReloadDelay секунд
        // Это позволит числу с очками долететь до места назначения
        Invoke("ReloadLevel", ReloadDelay);
    }

    private void ReloadLevel()
    {
        // Перезагрузить сцену и сбросить игру в исходное состояние
        SceneManager.LoadScene("Prospector Scene 0");
    }

    // Возвращает true, если две карты соответствуют правилу старшинства
    // (с учётом циклического переноса старшинства между тузом и королём)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // Если любая из карт повёрнута лицевой стороной вниз, правило старшинства не соблюдается
        if (!c0.FaceUp || !c1.FaceUp)
        {
            return false;
        }

        // Если достоинства карт отличаются на 1, правило старшинства соблюдается
        if (Mathf.Abs(c0.Rank - c1.Rank) == 1)
        {
            return true;
        }

        // Если одна карта - туз, а другая король, правило старшинства соблюдается
        if (c0.Rank == 1 && c1.Rank == 13)
        {
            return true;
        }
        if (c0.Rank == 13 && c1.Rank == 1)
        {
            return true;
        }

        // Иначе вернуть false
        return false;
    }

    // Обрабатывает движение FloatingScore
    private void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPoints;
        switch (evt)
        {
            // В случае победы, поражения и завершения хода выполняются одни и те же действия
            case eScoreEvent.Draw:       // Выбор свободной карты
            case eScoreEvent.GameWin:    // Победа в раунде
            case eScoreEvent.GameLoss:   // Поражение в раунде
                // Добавить FsRun в Scoreboard
                if (FsRun != null)
                {
                    // Создать точки для кривой Безье
                    fsPoints = new List<Vector2>();
                    fsPoints.Add(FsPosRun);
                    fsPoints.Add(FsPosMid2);
                    fsPoints.Add(FsPosEnd);
                    FsRun.ReportFinishTo = Scoreboard.S.gameObject;
                    FsRun.Init(fsPoints, 0, 1);
                    // Также скорректировать FontSize
                    FsRun.FontSizes = new List<float>(new float[] { 28, 36, 4 });
                    FsRun = null;   // Очистить FsRun, чтобы создать заново
                }
                break;

            case eScoreEvent.Mine:       // Удаление карты из основной раскладки
                // Создать FloatingScore для отображения этого количества очков
                FloatingScore floatingScore;
                // Переместить из позиции указателя мыши mousePosition в FsPosRun
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


    // Метод, решающий проблему преобразования локальных особенностей символов, связанной с CultureInfo
    private void Culturator()
    {
        CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        CultureInfo.CurrentCulture = cultureInfo;
    }
}
