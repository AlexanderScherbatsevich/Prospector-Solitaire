using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Xml;



public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool startFaceUp = false;
    //масти
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabCard;
    public GameObject prefabSprite;

    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    //вызывается экземпляром в Prospector
    public void InitDeck(string deckXMLText)
    {
        //создать точку привязки для всех игровых объектов Card в иерархии
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        //инициализировать словарь со спрайтами всех значков мастей
        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", suitClub },
            {"D", suitDiamond },
            {"H", suitHeart },
            {"S", suitSpade }
        };
        ReadDeck(deckXMLText);

        MakeCards();
    }

    //читает XML-файл и создает массив экземпляров CardDefinition
    public void ReadDeck(string deckXMLText)
    {
        NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);  //использовать его для чтения DeckXML

        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        //Прочитать элементы <decorator> для всех карт
        decorators = new List<Decorator>();
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"]; //извлечь список PT_XMLHashList всех элементов <decorator> из XML-файла
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++)
        {
            //для каждого элемента <decorator> в XMl
            deco = new Decorator();
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"), formatter);
            deco.loc.x = float.Parse(xDecos[i].att("x"),formatter);
            deco.loc.y = float.Parse(xDecos[i].att("y"), formatter);
            deco.loc.z = float.Parse(xDecos[i].att("z"), formatter);
            decorators.Add(deco);
        }
        //Прочитать координаты для значков, определяющих достоинство карты
        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];  //извлечь список PT_XMLHashList всех элементов <card> из XML-файла
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            //для каждого элемента <card>
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"), formatter);  //получить значение атрибута и добавить их в cDef
            //извлечь список PT_XMLHashList всех элементов <pip> внутри этого элемента <card>
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator(); //элементы <pip> в <card> обрабатываются классом Decorator
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"), formatter);
                    deco.loc.y = float.Parse(xPips[j].att("y"), formatter);
                    deco.loc.z = float.Parse(xPips[j].att("z"), formatter);
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"), formatter);
                    }
                    cDef.pips.Add(deco);
                }
            }
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }
    //получает CardDefinition на основе значения достоинства (1-14)
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        //поиск во всех определениях CardDefinition
        foreach (CardDefinition cd in cardDefs)
        {
            //если достоинство совладает, вернуть это определение
            if (cd.rank == rnk) return (cd);
        }
        return (null);
    }
    //создает игровые объекты карт
    public void MakeCards()
    {
        cardNames = new List<string>(); //содержит имена сконструированых карт
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        //создать список со всемии картами
        cards = new List<Card>();
        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }
    private Card MakeCard(int cNum)
    {         
        GameObject cgo = Instantiate(prefabCard) as GameObject;    //создать новый игровой объект с картой       
        cgo.transform.parent = deckAnchor;    //настроить transform.parent новой карты в соответствии с точкой привязки
        Card card = cgo.GetComponent<Card>();
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);  //выложить карты в аккуратный ряд

        //настроить основные параметры карты
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if (card.suit == "D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        if (card.suit == "C" || card.suit == "S")
        {
            card.colS = "Black";
            card.color = Color.black;
        }
        //получить CardDefinition для этой карты
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }
    //эти скрытые переменные используются вспомогательными методами
    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        //добавить оформление
        foreach (Decorator deco in decorators)
        {
            if (deco.type == "suit")
            {                
                _tGO = Instantiate(prefabSprite) as GameObject;  //создать экземпляр игрового объекта спрайта
                _tSR = _tGO.GetComponent<SpriteRenderer>();  //получить ссылку на компонент SpriteRenderer
                _tSR.sprite = dictSuits[card.suit];  //установить спрайт масти
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();               
                _tSp = rankSprites[card.rank];  //получить спрайт для отображения достоинства                
                _tSR.sprite = _tSp;             //установить спрайт достоинства в SpriteRenderer               
                _tSR.color = card.color;        //установить цвет соотвецтвующий масти
            }
            
            _tSR.sortingOrder = 1;  //поместить спрайты над картой          
            _tGO.transform.SetParent(card.transform);  //сделать  спрайт дочерним по отношению к карте           
            _tGO.transform.localPosition = deco.loc;  //установить localPosition как в DeckXML

            //перевернуть значок, если необходимо
            if (deco.flip)  
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);  //Эйлеров поворот на 180гр относительно оси Z-axis
            }
            //установить масштаб, что бы уменьшить размер спрайта
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            //дать имя и добавить в card.decoGOs
            _tGO.name = deco.type;
            card.decoGOs.Add(_tGO);
        }
    }
    private void AddPips(Card card)
    {
        //для каждого значка в определении
        foreach (Decorator pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite) as GameObject;  //создать игровой объект спрайта
            _tGO.transform.SetParent(card.transform);   //назначить родителем игровой объект карты
            _tGO.transform.localPosition = pip.loc;  //установить позицию из XML-файла
            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);   //перевернуть, если необходимо
            }
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;  //масштабировать, если необходимо(для туза)
            }
            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];   //установить спрайт масти
            _tSR.sortingOrder = 1;  //установить sortingOrder, чтобы значок отображался на Card_Front
            card.pipGOs.Add(_tGO);  //добавить в список значков
        }
    }
    private void AddFace(Card card)
    {
        if (card.def.face == "") return;   //выйти, если карта не с картинкой
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card.def.face + card.suit);  //сгенерировать имя и передать его в GetFace
        _tSR.sprite = _tSp;  //установить этот спрайт в _tSR
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }
    //находит спрайт с картинкой для карты
    private Sprite GetFace(string faceS)
    {
        foreach (Sprite _tSP in faceSprites)
        {
            //если спрайт с требуемым именем найден, вернуть его
            if (_tSP.name == faceS) return (_tSP);
        }
        return (null); //если ничего не найдено
    }
    private void AddBack(Card card)
    {
        //добавить рубашку, Card_Back будет покрывать все остальное на карте
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        //большее значение sortingOrder, чем у других спрайтов
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;

        //по умолчанию картинкой вверх
        card.faceUp = startFaceUp;
    }
    //перемешивает карты в Deck.cards
    static public void Shuffle(ref List<Card> oCards)
    {
        //создаем временный список для хранения карт в перемешанном порядке
        List<Card> tCards = new List<Card>();
        int ndx; //будет хранить индекс перемешанной карты
        tCards = new List<Card>();
        //повторять, пока не будут перемешаны все карты в исходном списке
        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);  //выбрать случайный индекс
            tCards.Add(oCards[ndx]);   //добавить эту карту во временный список
            oCards.RemoveAt(ndx);   //удалить эту карту из исходного списка
        }
        oCards = tCards; //заменить исходный список временным
    }
}
