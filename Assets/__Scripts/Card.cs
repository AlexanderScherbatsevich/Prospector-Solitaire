using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Globalization;



public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string suit; //масть карты(C,D,H,S)
    public int rank; //достоинство карты(1-14)
    public Color color = Color.black; //цвет значков
    public string colS = "Black"; //или "Red", им€ цвета

    public List<GameObject> decoGOs = new List<GameObject>(); //хранит все игровые объекты Decorator
    public List<GameObject> pipGOs = new List<GameObject>();  //хранит все игровые объекты Pip

    public GameObject back; //игровой объект рубашки карты

    public CardDefinition def; //извлекаетс€€ из DeckXML.xml

    public SpriteRenderer[] spriteRenderers;  //список компонентов SpriteRenderer этого и вложенных в него игровых объектов
    private void Start()
    {
        SetSortOrder(0);  //обеспечивает правильную сортировку карт
    }

    //если spriteRenderers не определен, эта фенкци€ определит его
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();  //получить компоненты этого объекта и его дочерних объектов
        }
    }
    //инициализирует поле sortingLayerName во всех компонентах SpriteRenderer
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }
    //инициализирует поле sortingOrder всех компонентов SpriteRenderer
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if (tSR.gameObject == this.gameObject) //если компонент принадлежит текущему объекту. это фон
            {
                tSR.sortingOrder = sOrd;  //установить пор€дковый номер дл€ сортировки в sOrd
                continue;
            }
            //установить пор€дковый номер дл€ сортировки, в зависимости от имени
            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd + 2;  //установить наибольший пор€дковый номер, дл€ отображени€ поверх других спрайтов
                    break;
                case "face":    //если им€ face или другое
                default:
                    tSR.sortingOrder = sOrd + 1;  //установить промежуточный пор€дковый номер, дл€ отображени€ поверх фона
                    break;           
            }
        }
    }
    public bool faceUp
    {
        get { return (!back.activeSelf); }
        set { back.SetActive(!value); }
    }
    virtual public void OnMouseUpAsButton()
    {
        print(name);
    }
}

[System.Serializable]
public class Decorator
{
    //этот класс хранит информацию из DeckXML о каждом значке на карте
    public string type;   //значок, определ€ющий достоинство карты, имеет type = "pip"
    public Vector3 loc;   //местоположение спрайта на карте
    public bool flip = false;  //признак переворота спрайта по вертекали
    public float scale = 1f;   //масштаб спрайта
}

[System.Serializable]
public class CardDefinition
{
    //этот класс хранит информацию о достоинстве карты
    public string face;     //спрайт, изображающий лицевую сторону карты
    public int rank;        //достоинство карты (1-13)
    public List<Decorator> pips = new List<Decorator>();    //значки
}