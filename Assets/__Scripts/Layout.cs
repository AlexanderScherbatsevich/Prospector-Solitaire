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
    public PT_XMLHashtable xml;  //используется для ускорения доступа к xml
    public Vector2 multiplier;  //смещение от центра раскладки
    //ссылки SlotDef
    public List<SlotDef> slotDefs;  //все экземпляры SlotDef для рядов 0-3
    public SlotDef drawPile;
    public SlotDef discardPile;
    //хранит имена всех рядов
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    //функция для чтения файла LayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);  //загрузить XML
        xml = xmlr.xml["xml"][0];  //определить xml для ускорения доступа к XML

        //прочитать множители, определяющие расстояние между картами
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), formatter);
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), formatter);

        //прочитать слоты
        SlotDef tSD;
        PT_XMLHashList slotsX = xml["slot"];  //используется для ускорения доступа к элементам <slot>

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();
            if (slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");  //если <slot> имеет атрибут type, прочитать его
            }
            else
            {
                tSD.type = "slot";  //иначе определить тип как "slot", это отдельная карта в ряду
            }
            //преобразовать некоторые атрибуты в числовые значения
            tSD.x = float.Parse(slotsX[i].att("x"),formatter);
            tSD.y = float.Parse(slotsX[i].att("y"), formatter);
            tSD.layerID = int.Parse(slotsX[i].att("layer"), formatter);
            tSD.layerName = sortingLayerNames[tSD.layerID];  //преобразовать номер ряда в текст

            switch (tSD.type)
            {
                //прочитать дополнительные атрибуты, опираясь на тип слота
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
