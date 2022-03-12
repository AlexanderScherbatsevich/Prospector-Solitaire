using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Xml;

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}
public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;  //������������ ��� ��������� ������� � xml
    public Vector2 multiplier;  //�������� �� ������ ���������
    //������ SlotDef
    public List<SlotDef> slotDefs;  //��� ���������� SlotDef ��� ����� 0-3
    public SlotDef drawPile;
    public SlotDef discardPile;
    //������ ����� ���� �����
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    //������� ��� ������ ����� LayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);  //��������� XML
        xml = xmlr.xml["xml"][0];  //���������� xml ��� ��������� ������� � XML

        //��������� ���������, ������������ ���������� ����� �������
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), formatter);
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), formatter);

        //��������� �����
        SlotDef tSD;
        PT_XMLHashList slotsX = xml["slot"];  //������������ ��� ��������� ������� � ��������� <slot>

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();
            if (slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");  //���� <slot> ����� ������� type, ��������� ���
            }
            else
            {
                tSD.type = "slot";  //����� ���������� ��� ��� "slot", ��� ��������� ����� � ����
            }
            //������������� ��������� �������� � �������� ��������
            tSD.x = float.Parse(slotsX[i].att("x"),formatter);
            tSD.y = float.Parse(slotsX[i].att("y"), formatter);
            tSD.layerID = int.Parse(slotsX[i].att("layer"), formatter);
            tSD.layerName = sortingLayerNames[tSD.layerID];  //������������� ����� ���� � �����

            switch (tSD.type)
            {
                //��������� �������������� ��������, �������� �� ��� �����
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"), formatter);
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), formatter);
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}
