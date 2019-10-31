using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameParamType : MonoBehaviour
{
    [HideInInspector]
    public Proto.GameParamUIType uiType;

    [HideInInspector]
    public Proto.paramType paramType;
}
