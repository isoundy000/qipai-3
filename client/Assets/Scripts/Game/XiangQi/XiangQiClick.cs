using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class XiangQiClick : MonoBehaviour, IPointerClickHandler
{
    int _width = 50;
    RectTransform meRect;

    public void Init(int offset, int width)
    {
        meRect = GetComponent<RectTransform>();
        _width = width;
        int tmpWidth = 8 * width + 2 * offset;
        int tmpHeight = 9 * width + 2 * offset;
        Debug.Log(tmpWidth + "/" + tmpHeight);
        meRect.sizeDelta = new Vector2(tmpWidth, tmpHeight);
        meRect.anchoredPosition = new Vector2(-tmpWidth / 2, tmpHeight / 2);
        transform.parent.Find("sign").GetComponent<RectTransform>().anchoredPosition = new Vector2(-4 * width, 4.5f * width);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(meRect,
            eventData.position, eventData.pressEventCamera, out localPos);
        int i = (int)(Mathf.Abs(localPos.y) / _width);
        int j = (int)(Mathf.Abs(localPos.x) / _width);
        XiangQiPanel.instance.QiPanClick(i, j);
    }
}
