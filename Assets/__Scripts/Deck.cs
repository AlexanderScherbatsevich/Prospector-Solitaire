using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Xml;



public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool startFaceUp = false;
    //�����
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

    //���������� ����������� � Prospector
    public void InitDeck(string deckXMLText)
    {
        //������� ����� �������� ��� ���� ������� �������� Card � ��������
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        //���������������� ������� �� ��������� ���� ������� ������
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

    //������ XML-���� � ������� ������ ����������� CardDefinition
    public void ReadDeck(string deckXMLText)
    {
        NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);  //������������ ��� ��� ������ DeckXML

        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        //��������� �������� <decorator> ��� ���� ����
        decorators = new List<Decorator>();
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"]; //������� ������ PT_XMLHashList ���� ��������� <decorator> �� XML-�����
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++)
        {
            //��� ������� �������� <decorator> � XMl
            deco = new Decorator();
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"), formatter);
            deco.loc.x = float.Parse(xDecos[i].att("x"),formatter);
            deco.loc.y = float.Parse(xDecos[i].att("y"), formatter);
            deco.loc.z = float.Parse(xDecos[i].att("z"), formatter);
            decorators.Add(deco);
        }
        //��������� ���������� ��� �������, ������������ ����������� �����
        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];  //������� ������ PT_XMLHashList ���� ��������� <card> �� XML-�����
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            //��� ������� �������� <card>
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"), formatter);  //�������� �������� �������� � �������� �� � cDef
            //������� ������ PT_XMLHashList ���� ��������� <pip> ������ ����� �������� <card>
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator(); //�������� <pip> � <card> �������������� ������� Decorator
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
    //�������� CardDefinition �� ������ �������� ����������� (1-14)
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        //����� �� ���� ������������ CardDefinition
        foreach (CardDefinition cd in cardDefs)
        {
            //���� ����������� ���������, ������� ��� �����������
            if (cd.rank == rnk) return (cd);
        }
        return (null);
    }
    //������� ������� ������� ����
    public void MakeCards()
    {
        cardNames = new List<string>(); //�������� ����� ���������������� ����
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        //������� ������ �� ������ �������
        cards = new List<Card>();
        for (int i = 0; i < cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }
    private Card MakeCard(int cNum)
    {         
        GameObject cgo = Instantiate(prefabCard) as GameObject;    //������� ����� ������� ������ � ������       
        cgo.transform.parent = deckAnchor;    //��������� transform.parent ����� ����� � ������������ � ������ ��������
        Card card = cgo.GetComponent<Card>();
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);  //�������� ����� � ���������� ���

        //��������� �������� ��������� �����
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
        //�������� CardDefinition ��� ���� �����
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }
    //��� ������� ���������� ������������ ���������������� ��������
    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        //�������� ����������
        foreach (Decorator deco in decorators)
        {
            if (deco.type == "suit")
            {                
                _tGO = Instantiate(prefabSprite) as GameObject;  //������� ��������� �������� ������� �������
                _tSR = _tGO.GetComponent<SpriteRenderer>();  //�������� ������ �� ��������� SpriteRenderer
                _tSR.sprite = dictSuits[card.suit];  //���������� ������ �����
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();               
                _tSp = rankSprites[card.rank];  //�������� ������ ��� ����������� �����������                
                _tSR.sprite = _tSp;             //���������� ������ ����������� � SpriteRenderer               
                _tSR.color = card.color;        //���������� ���� �������������� �����
            }
            
            _tSR.sortingOrder = 1;  //��������� ������� ��� ������          
            _tGO.transform.SetParent(card.transform);  //�������  ������ �������� �� ��������� � �����           
            _tGO.transform.localPosition = deco.loc;  //���������� localPosition ��� � DeckXML

            //����������� ������, ���� ����������
            if (deco.flip)  
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);  //������� ������� �� 180�� ������������ ��� Z-axis
            }
            //���������� �������, ��� �� ��������� ������ �������
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            //���� ��� � �������� � card.decoGOs
            _tGO.name = deco.type;
            card.decoGOs.Add(_tGO);
        }
    }
    private void AddPips(Card card)
    {
        //��� ������� ������ � �����������
        foreach (Decorator pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite) as GameObject;  //������� ������� ������ �������
            _tGO.transform.SetParent(card.transform);   //��������� ��������� ������� ������ �����
            _tGO.transform.localPosition = pip.loc;  //���������� ������� �� XML-�����
            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);   //�����������, ���� ����������
            }
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;  //��������������, ���� ����������(��� ����)
            }
            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];   //���������� ������ �����
            _tSR.sortingOrder = 1;  //���������� sortingOrder, ����� ������ ����������� �� Card_Front
            card.pipGOs.Add(_tGO);  //�������� � ������ �������
        }
    }
    private void AddFace(Card card)
    {
        if (card.def.face == "") return;   //�����, ���� ����� �� � ���������
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card.def.face + card.suit);  //������������� ��� � �������� ��� � GetFace
        _tSR.sprite = _tSp;  //���������� ���� ������ � _tSR
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }
    //������� ������ � ��������� ��� �����
    private Sprite GetFace(string faceS)
    {
        foreach (Sprite _tSP in faceSprites)
        {
            //���� ������ � ��������� ������ ������, ������� ���
            if (_tSP.name == faceS) return (_tSP);
        }
        return (null); //���� ������ �� �������
    }
    private void AddBack(Card card)
    {
        //�������� �������, Card_Back ����� ��������� ��� ��������� �� �����
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        //������� �������� sortingOrder, ��� � ������ ��������
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;

        //�� ��������� ��������� �����
        card.faceUp = startFaceUp;
    }
    //������������ ����� � Deck.cards
    static public void Shuffle(ref List<Card> oCards)
    {
        //������� ��������� ������ ��� �������� ���� � ������������ �������
        List<Card> tCards = new List<Card>();
        int ndx; //����� ������� ������ ������������ �����
        tCards = new List<Card>();
        //���������, ���� �� ����� ���������� ��� ����� � �������� ������
        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);  //������� ��������� ������
            tCards.Add(oCards[ndx]);   //�������� ��� ����� �� ��������� ������
            oCards.RemoveAt(ndx);   //������� ��� ����� �� ��������� ������
        }
        oCards = tCards; //�������� �������� ������ ���������
    }
}
