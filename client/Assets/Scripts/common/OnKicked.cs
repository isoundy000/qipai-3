using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnKicked : MonoBehaviour
{

    public Text infoText;
    public void Init(string info)
    {
        infoText.text = info;
    }

    public void Btn_Yes()
    {
        SceneManager.LoadScene(SceneNames.login);
    }
}
