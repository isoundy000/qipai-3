using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuger
{
    public static bool EnableLog = true;

    #region Log
    public static void Log(object message)
    {
        Log(message, null);
    }

    public static void Log(object message, Object context)
    {
        if (EnableLog)
        {
            Debug.Log(message, context);
        }
    }

    public static void Log(object message, object message2, Object context)
    {
        if (EnableLog)
        {
            Debug.Log(string.Format("{0} {1}", message, message2), context);
        }
    }

    public static void Log(params object[] values)
    {
        if (EnableLog)
        {
            string tmpStr = "";
            for (int i = 0; i < values.Length; i++)
            {
                tmpStr += "{" + i + "} ";
            }
            Debug.LogFormat(tmpStr, values);
        }
    }
    #endregion



    #region LogWarning
    public static void LogWarning(object message)
    {
        LogWarning(message, null);
    }

    public static void LogWarning(object message, Object context)
    {
        if (EnableLog)
        {
            Debug.LogWarning(message, context);
        }
    }

    public static void LogWarning(object message, object message2, Object context)
    {
        if (EnableLog)
        {
            Debug.LogWarning(string.Format("{0} {1}", message, message2), context);
        }
    }

    public static void LogWarning(params object[] values)
    {
        if (EnableLog)
        {
            string tmpStr = "";
            for (int i = 0; i < values.Length; i++)
            {
                tmpStr += "{" + i + "} ";
            }
            Debug.LogWarningFormat(tmpStr, values);
        }
    }
    #endregion



    #region LogError
    public static void LogError(object message)
    {
        LogError(message, null);
    }

    public static void LogError(object message, Object context)
    {
        if (EnableLog)
        {
            Debug.LogError(message, context);
        }
    }

    public static void LogError(object message, object message2, Object context)
    {
        if (EnableLog)
        {
            Debug.LogError(string.Format("{0} {1}", message, message2), context);
        }
    }

    public static void LogError(params object[] values)
    {
        if (EnableLog)
        {
            string tmpStr = "";
            for (int i = 0; i < values.Length; i++)
            {
                tmpStr += "{" + i + "} ";
            }
            Debug.LogErrorFormat(tmpStr, values);
        }
    }
    #endregion


}
