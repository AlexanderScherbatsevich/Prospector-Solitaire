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

//FloatingScore может перемещаться на экране по траектории, которая определяется кривой Безье
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
            scoreString = _score.ToString("N0");  //аргумент N0 требует добавить точки в число
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;  //точки определяющие кривую Безье
    public List<float> fontSizes;   //точки Безье для масштабирования шрифта
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;  //функция сглаживания из Utils.cs

    //игровой объект, для которого будет вызван метод SendMessage, когда этот экземпляр FloatingScore закончит движение
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private Text txt;

    //настройка FloatingScore и парамерты движения
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);

        //если задана только одна точка, переместиться в нее
        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        //если eTimeS имеет значение по умолчанию, запустить отсчет от текущего времени
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSState.pre;  //установить значение pre - готовность начать движение
    }

    public void FSCallback(FloatingScore fs)
    {
        //когда SendMessage вызовет эту функцию, она должна добавить очки из вызвавшего экземпляра FloatingScore
        score += fs.score;
    }
    private void Update()
    {
        //если этот объект никуда не перемещается, просто выйти
        if (state == eFSState.idle) return;

        //вычислить u на основа текущего времени и продолжительности движения, u изменяется от 0 до 1
        float u = (Time.time - timeStart) / timeDuration;
        //используем класс Easing из Utils.cs для корректировки значения u
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        {
            //если u<0 объект не должен двигаться
            state = eFSState.pre;
            txt.enabled = false;   //изначально скрыть число
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;   //чтобы не выйти за крайнюю точку
                state = eFSState.post;
                if (reportFinishTo != null)  
                {
                    //если игровой объект указан, использовать SendMessage для вызова FSCallback
                    // и передачи ему текущего экземпляра в параметре
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
                //если 0<=u<1, значит текущий экземпляр активен и движется
                state = eFSState.active;
                txt.enabled = true;  //показать число очков
            }
            //использовать кривую Безье для перемещения к заданной точке
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            //опорные точки RectTransform использовать для позицианирования объектов UI относительно общего размера экрана
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                //скорректировать fontSizes этого объекта GUIText
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
