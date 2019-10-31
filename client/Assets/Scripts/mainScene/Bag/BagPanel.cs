using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagPanel : MonoBehaviour
{
    public static BagPanel instance = null;

    public Text itemNameText;
    public Text itemInfoText;
    public Transform itemParent;
    public GameObject itemPrefab;
    private ToggleGroup itemToggleGroup;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        itemToggleGroup = itemParent.GetComponent<ToggleGroup>();
        bool isFirst = true;
        foreach (int key in PlayerInfo.bagInfo.Keys)
        {
            Transform tmpTran = Instantiate(itemPrefab, itemParent).transform;
            tmpTran.GetComponent<BagItemPrefab>().Init(key, PlayerInfo.bagInfo[key]);
            tmpTran.GetComponent<Toggle>().group = itemToggleGroup;
            if (isFirst)
            {
                isFirst = false;
                tmpTran.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    public void OnItemClick(int itemId)
    {
        itemNameText.text = itemId.ToString();
        itemInfoText.text = "道具" + itemId.ToString();
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

}
