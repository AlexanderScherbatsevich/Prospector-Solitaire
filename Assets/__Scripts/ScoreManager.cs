using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//перечисление со всеми возможными событиями начисления очков
public enum eScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    //поля для хранения информации о заработаных очках
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    private void Awake()
    {
        if (S == null) 
        {
            S = this;
        }
        else
        {
            Debug.LogError("ERROR: ScoreManager.Awake(): S is already set!");
        }
        //проверить рекорд в PlayerPrefs
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        //добавить заработанные очки в последнем раунде, которы е должны быть >0, если раунд завершился победой
        score += SCORE_FROM_PREV_ROUND;
        //сбросить счет
        SCORE_FROM_PREV_ROUND = 0;
    }
    static public void EVENT(eScoreEvent evt)
    {
        //не позволит ошибке аварийно завершить игру
        try
        {
            S.Event(evt);
        }catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager:EVENT() called while S=null.\n" + nre);
        }
    }
    private void Event(eScoreEvent evt)
    {
        switch (evt)
        {
            //в случае пробеды, проигрыша и завершения хода выполняются одни и те же действия
            case eScoreEvent.draw:      //выбор свободной карты
            case eScoreEvent.gameWin:   //победа в раунде
            case eScoreEvent.gameLoss:  //проигрыш в раунде
                chain = 0;              //сбросить цепочку  подсчета очков
                score += scoreRun;      //добавить scoreRun к общему числу очков
                scoreRun = 0;           
                break;

            case eScoreEvent.mine:      //удаление карты из основной раскладки
                chain++;                //увеличить количество очков в цепочке
                scoreRun += chain;      //добавить очки за карту
                break;
        }
        //обрабатывает победу и проигрыш в раунде
        switch (evt)
        {
            case eScoreEvent.gameWin:
                //в случае победы перенести очки в след. раунд
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round. Round score" + score);
                break;
            case eScoreEvent.gameLoss:
                //при проигрыше сравнить с рекордом
                if (HIGH_SCORE <= score)
                {
                    print("you got the high score" + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("your final score for game was" + score);
                }
                break;
            default:
                print("score:" + score + "scoreRun:" + scoreRun + "chain:" + chain);
                break;
        }
    }
    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}
