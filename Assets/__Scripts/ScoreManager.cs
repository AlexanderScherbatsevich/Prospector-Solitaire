using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//������������ �� ����� ���������� ��������� ���������� �����
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
    //���� ��� �������� ���������� � ����������� �����
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
        //��������� ������ � PlayerPrefs
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        //�������� ������������ ���� � ��������� ������, ������ � ������ ���� >0, ���� ����� ���������� �������
        score += SCORE_FROM_PREV_ROUND;
        //�������� ����
        SCORE_FROM_PREV_ROUND = 0;
    }
    static public void EVENT(eScoreEvent evt)
    {
        //�� �������� ������ �������� ��������� ����
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
            //� ������ �������, ��������� � ���������� ���� ����������� ���� � �� �� ��������
            case eScoreEvent.draw:      //����� ��������� �����
            case eScoreEvent.gameWin:   //������ � ������
            case eScoreEvent.gameLoss:  //�������� � ������
                chain = 0;              //�������� �������  �������� �����
                score += scoreRun;      //�������� scoreRun � ������ ����� �����
                scoreRun = 0;           
                break;

            case eScoreEvent.mine:      //�������� ����� �� �������� ���������
                chain++;                //��������� ���������� ����� � �������
                scoreRun += chain;      //�������� ���� �� �����
                break;
        }
        //������������ ������ � �������� � ������
        switch (evt)
        {
            case eScoreEvent.gameWin:
                //� ������ ������ ��������� ���� � ����. �����
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round. Round score" + score);
                break;
            case eScoreEvent.gameLoss:
                //��� ��������� �������� � ��������
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
