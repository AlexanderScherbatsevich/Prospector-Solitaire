using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml;
using System.Globalization;




public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    //public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 10f;
    public Text gameOverText, roundResultText, highScoreText;
    public GameObject menu;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;

    private void Awake()
    {
        S = this;
        //SetUpUITexts();
    }
    void SetUpUITexts()
    {
        //настроить объект HighScore
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;

        //настроить надписи, отображаемые в конце раунда
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        //скрыть надписи
        ShowResultsUI(false);
    }
    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    private void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

        deck = GetComponent<Deck>();             //получить компонент Deck
        deck.InitDeck(deckXML.text);   //передать ему DeckXML
        Deck.Shuffle(ref deck.cards);   //перемешать колоду, передав ее по ссылке
        SetUpUITexts();

        //Card c;
        //for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        //{
        //    c = deck.cards[cNum];
        //    c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }
    public void Shuffle()
    {
        Deck.Shuffle(ref deck.cards);
    }
    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }
    //функция Draw снимает однну карту с вершины drawPile и возвращает ее
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }
    //LayoutGame() размещает карты в начальной раскладке - "шахте"
    void LayoutGame()
    {
        //создать пустой игровой объект, как центр раскладки
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        CardProspector cp;
        //разложить карты
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();  //выбрать первую карту (сверху) из стопки drawPile
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            //заменить предыдущего родителя: deck_Anchor, который после запуска игры отображается как _Deck
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y,
                -tSD.layerID);  //установить позицию в соответствии с SlotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            //карты CardProspector в основной раскладке имеют состояние CardState.tableau
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);  //назначить слой сортировки
            tableau.Add(cp);
        }

        //настроить списки карт, мешающих перевернуть данную
        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }
        //выбрать начальную целевую карту 
        MoveToTarget(Draw());

        //разложить стопку свободных карт
        UpdateDrawPile();
    }

    //преобразует номер слота layoutID в экземпляр CardProspector с этим номером
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            if (tCP.layoutID == layoutID) return (tCP);  //если номер карты совпадает с искомым, вернуть ее
        }
        return (null);
    }

    //поворачивает карты в основной раскладке лицевой стороной вверх или вниз 
    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool faceUp = true;   //предположить, что карта должна быть повернута лицевой стороной вверх
            foreach (CardProspector cover in cd.hiddenBy)
            {
                //если любая из карт, перекрывающая текущую, присутствует в раскладке, повернуть лицевой стороной вниз
                if (cover.state == eCardState.tableau) faceUp = false;  
            }
            cd.faceUp = faceUp;
        }
    }
    //перемещает текущую целевую карту в стопку сброшенных карт
    void MoveToDiscard(CardProspector cd)
    {
        //установить состояние карты как discard (сброшена)
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;  //обновить значение

        //переместить эту карту с позицию стопки сброшенных карт
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);

        cd.faceUp = true;
        //поместить поверх стопки для сортировки по глубине
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    //делает карту cd новой целевой картой
    void MoveToTarget(CardProspector cd)
    {
        //если целевая карта существует, переместить ее в стопку сброшенных карт
        if (target != null) MoveToDiscard(target);
        target = cd;   //cd новая целевая карта
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        //переместить ее на место для целевой карты
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);

        cd.faceUp = true;  //перевернуть лицевой стороной вверх
        //настроить сортировку по глубине
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    //раскладывает стопку свободных карт, чтобы было видно сколько карт осталось
    void UpdateDrawPile()
    {
        CardProspector cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            //расположить с учетом смещения layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);

            cd.faceUp = false;   //перевернуть лицевой стороной вниз   
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    //вызывается в ответ на щелчок на любой карте
    public void CardClicked(CardProspector cd)
    {
        //реакция определяется состоянием карты
        switch (cd.state)
        {
            case eCardState.target:   //щелчок игнорируется
                break;
            case eCardState.drawpile:
                //щелчок на любой карте в стопке свободных карт приводит к смене целевой карты
                MoveToDiscard(target);   //переместить целевую карту в discardPile
                MoveToTarget(Draw());   //переместить верхнюю свободную карту на место целевой
                UpdateDrawPile();   //повторно разложить стопку свободных карт
                AudioManager.S.cardFromDrawpile.Play();
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                //для карты в основной раскладке проверяется возможность ее перемещения на место целевой
                bool validMatch = true;

                if (!cd.faceUp) validMatch = false; //карта, повернутая лицевой стороной вниз, не может перемещаться
                if (!AdjacentRank(cd, target)) validMatch = false;  //если правило старшинства не соблюдается, карта не может перемещаться
                if (!validMatch) return;

                //карту можно переместить
                tableau.Remove(cd);
                MoveToTarget(cd);  //сделать карту целевой
                SetTableauFaces();  //повернуть карты в основной раскладке лицевой стороной вверх или вниз
                AudioManager.S.cardFromTableau.Play();
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
        }
        //проверить завершение игры
        CheckForGameOver();
    }

    //проверяет завершение игры
    void CheckForGameOver()
    {
        //если основная раскладка опустела, игра завершена, вызвать GameOver в признаком победы
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }

        //если еще есть свободные карты, игра не завершилась
        if (drawPile.Count > 0) return;

        //проверить наличие допустимых ходов
        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target)) return;  //если есть допустимый ход, игра не завершилась
        }

        //если допустимых ходов нет, игра завершилась, вызвать GameOver с признаком проигрыша
        GameOver(false);
    }

    //вызывается когда игра завершилась
    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;

        if (won)
        {
            AudioManager.S.Music.audioMixer.SetFloat("MusicVolume", -80);
            AudioManager.S.soundWin.Play();
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            //print("GameOver. You won!");
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
            Invoke("ReloadLevel", reloadDelay);
        }
        else
        {
            AudioManager.S.Music.audioMixer.SetFloat("MusicVolume", -80);
            AudioManager.S.soundLose.Play();
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh Score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            //print("GameOver. You lost.");
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
            Invoke("ReloadNewGame", reloadDelay);
        }
        //перезагрузить сцену и сбросить игру в исходное состояние 
        //SceneManager.LoadScene("__Prospector_Scene_0");
        //Invoke("ReloadLevel", reloadDelay);
    }
    void ReloadLevel()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }
    void ReloadNewGame()
    {
        menu.SetActive(true);
    }

    //возвращает true, если две карты соответствуют правилу старшинства
    //с учетом циклического переноса старшинства между тузом и королем
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        //если любая из карт повернута лицевой стороной вниз, правило старшинства не соблюдается
        if (!c0.faceUp || !c1.faceUp) return (false);

        //если достоинства карт отличаются на 1, правило старшинства соблюдается
        if (Mathf.Abs(c0.rank - c1.rank) == 1) return (true);

        //если одна карта - туз, а другая - король, правило старшинства соблюдается
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);

        //иначе вернуть false
        return (false);
    }
    //обрабатывает движение FloatingScore
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                //добавить fsRun  в Scoreboard
                if (fsRun != null)
                {
                    //создать точки кривой Безье
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    //скорректировать fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 100, 170, 10 });
                    fsRun = null;  //очистить, чтобы создать заново
                }
                break;
                //удаление карты из основной раскладки
            case eScoreEvent.mine:
                FloatingScore fs;  //создать для отображения этого количества очков
                Vector2 p0 = Input.mousePosition;  //переместить из позиции указателя мыши в fsPosRun
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 10, 170, 100 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }
}
