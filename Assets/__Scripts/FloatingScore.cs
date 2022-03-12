using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eFSState
{
    idle,
    pre,
    active,
    post
}

//FloatingScore ����� ������������ �� ������ �� ����������, ������� ������������ ������ �����
public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    public int score
    {
        get { return (_score); }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");  //�������� N0 ������� �������� ����� � �����
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;  //����� ������������ ������ �����
    public List<float> fontSizes;   //����� ����� ��� ��������������� ������
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;  //������� ����������� �� Utils.cs

    //������� ������, ��� �������� ����� ������ ����� SendMessage, ����� ���� ��������� FloatingScore �������� ��������
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private Text txt;

    //��������� FloatingScore � ��������� ��������
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);

        //���� ������ ������ ���� �����, ������������� � ���
        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        //���� eTimeS ����� �������� �� ���������, ��������� ������ �� �������� �������
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSState.pre;  //���������� �������� pre - ���������� ������ ��������
    }

    public void FSCallback(FloatingScore fs)
    {
        //����� SendMessage ������� ��� �������, ��� ������ �������� ���� �� ���������� ���������� FloatingScore
        score += fs.score;
    }
    private void Update()
    {
        //���� ���� ������ ������ �� ������������, ������ �����
        if (state == eFSState.idle) return;

        //��������� u �� ������ �������� ������� � ����������������� ��������, u ���������� �� 0 �� 1
        float u = (Time.time - timeStart) / timeDuration;
        //���������� ����� Easing �� Utils.cs ��� ������������� �������� u
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        {
            //���� u<0 ������ �� ������ ���������
            state = eFSState.pre;
            txt.enabled = false;   //���������� ������ �����
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;   //����� �� ����� �� ������� �����
                state = eFSState.post;
                if (reportFinishTo != null)  
                {
                    //���� ������� ������ ������, ������������ SendMessage ��� ������ FSCallback
                    // � �������� ��� �������� ���������� � ���������
                    reportFinishTo.SendMessage("FSCallback", this);
                    Destroy(this.gameObject);
                }
                else
                {
                    state = eFSState.idle;
                }
            }
            else
            {
                //���� 0<=u<1, ������ ������� ��������� ������� � ��������
                state = eFSState.active;
                txt.enabled = true;  //�������� ����� �����
            }
            //������������ ������ ����� ��� ����������� � �������� �����
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            //������� ����� RectTransform ������������ ��� ���������������� �������� UI ������������ ������ ������� ������
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                //��������������� fontSizes ����� ������� GUIText
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
