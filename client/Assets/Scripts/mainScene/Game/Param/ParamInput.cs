using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParamInput : GameParamType
{
    private I_param_input paramData;
    private InputField input;
    public void Init(I_param_input val)
    {
        paramData = val;
        paramType = val.type;
        uiType = Proto.GameParamUIType.input;
        transform.Find("Text").GetComponent<Text>().text = PlayerInfo.gameParamName[paramData.type] + ":";
        input = transform.Find("InputField").GetComponent<InputField>();
        if (paramData.isNumber)
        {
            input.contentType = InputField.ContentType.IntegerNumber;
        }
        transform.Find("InputField/Placeholder").GetComponent<Text>().text = paramData.placeholder;
    }
    public bool IfNumOk()
    {
        if (!paramData.isNumber)
        {
            return true;
        }
        int num = 0;
        if (!int.TryParse(input.text, out num))
        {
            UIManager.instance.SetTileInfo(string.Format("\"{0}\"必须在{1}和{2}之间", PlayerInfo.gameParamName[paramData.type], paramData.min, paramData.max));
            return false;
        }
        if (num < paramData.min || num > paramData.max)
        {
            UIManager.instance.SetTileInfo(string.Format("\"{0}\"必须在{1}和{2}之间", PlayerInfo.gameParamName[paramData.type], paramData.min, paramData.max));
            return false;
        }
        return true;
    }

    public string GetValue()
    {
        return input.text;
    }
}
