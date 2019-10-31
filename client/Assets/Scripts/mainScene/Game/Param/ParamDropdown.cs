using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParamDropdown : GameParamType
{
    private I_param_dropdown paramData;
    private int index = 0;
    public void Init(I_param_dropdown val)
    {
        paramData = val;
        paramType = paramData.type;
        uiType = Proto.GameParamUIType.dropdown;
        transform.Find("Text").GetComponent<Text>().text = PlayerInfo.gameParamName[val.type] + ":";
        var dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < paramData.values.Count; i++)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = paramData.values[i];
            options.Add(data);
        }
        dropdown.AddOptions(options);
        dropdown.SetValueWithoutNotify(paramData.defaultIndex);
        Dropdown_changed(paramData.defaultIndex);
    }

    public int GetIndex()
    {
        return index;
    }

    public void Dropdown_changed(int _index)
    {
        index = _index;
        if (paramData.type == Proto.paramType.rankNum)
        {
            string str = paramData.values[index];
            int rankNum = int.Parse(str.Substring(0, str.Length - 1));
            if (CreateGamePanel.instance != null)
            {
                CreateGamePanel.instance.SetRankNum(rankNum);
            }
        }
    }
}
