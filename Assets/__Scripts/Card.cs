using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Globalization;



public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string suit; //����� �����(C,D,H,S)
    public int rank; //����������� �����(1-14)
    public Color color = Color.black; //���� �������
    public string colS = "Black"; //��� "Red", ��� �����

    public List<GameObject> decoGOs = new List<GameObject>(); //������ ��� ������� ������� Decorator
    public List<GameObject> pipGOs = new List<GameObject>();  //������ ��� ������� ������� Pip

    public GameObject back; //������� ������ ������� �����

    public CardDefinition def; //������������ �� DeckXML.xml

    public SpriteRenderer[] spriteRenderers;  //������ ����������� SpriteRenderer ����� � ��������� � ���� ������� ��������
    private void Start()
    {
        SetSortOrder(0);  //������������ ���������� ���������� ����
    }

    //���� spriteRenderers �� ���������, ��� ������� ��������� ���
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();  //�������� ���������� ����� ������� � ��� �������� ��������
        }
    }
    //�������������� ���� sortingLayerName �� ���� ����������� SpriteRenderer
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }
    //�������������� ���� sortingOrder ���� ����������� SpriteRenderer
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if (tSR.gameObject == this.gameObject) //���� ��������� ����������� �������� �������. ��� ���
            {
                tSR.sortingOrder = sOrd;  //���������� ���������� ����� ��� ���������� � sOrd
                continue;
            }
            //���������� ���������� ����� ��� ����������, � ����������� �� �����
            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd + 2;  //���������� ���������� ���������� �����, ��� ����������� ������ ������ ��������
                    break;
                case "face":    //���� ��� face ��� ������
                default:
                    tSR.sortingOrder = sOrd + 1;  //���������� ������������� ���������� �����, ��� ����������� ������ ����
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
    //���� ����� ������ ���������� �� DeckXML � ������ ������ �� �����
    public string type;   //������, ������������ ����������� �����, ����� type = "pip"
    public Vector3 loc;   //�������������� ������� �� �����
    public bool flip = false;  //������� ���������� ������� �� ���������
    public float scale = 1f;   //������� �������
}

[System.Serializable]
public class CardDefinition
{
    //���� ����� ������ ���������� � ����������� �����
    public string face;     //������, ������������ ������� ������� �����
    public int rank;        //����������� ����� (1-13)
    public List<Decorator> pips = new List<Decorator>();    //������
}