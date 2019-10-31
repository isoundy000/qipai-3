using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QiPanClick : MonoBehaviour, IPointerClickHandler
{
    RectTransform meRect;
    int _width = 50;

    public void Init(int offset, int width)
    {
        meRect = GetComponent<RectTransform>();
        meRect.anchoredPosition = new Vector2(-offset, offset);
        int tmpWidth = width * 18 + offset * 2;
        meRect.sizeDelta = new Vector2(tmpWidth, tmpWidth);
        _width = width;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(meRect,
            eventData.position, eventData.pressEventCamera, out localPos);
        int i = (int)(Mathf.Abs(localPos.y) / _width);
        int j = (int)(Mathf.Abs(localPos.x) / _width);
        WuZiQiPanel.instance.QiPanClick(localPos.x, localPos.y, i, j);
    }
}
