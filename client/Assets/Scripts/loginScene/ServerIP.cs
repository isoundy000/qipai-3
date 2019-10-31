using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerIP : MonoBehaviour
{
    public static ServerIP instance;

    public int defaultIpIndex = 0;
    private List<LoginIP> ipList = new List<LoginIP>()
    {
        new LoginIP( "本机", "127.0.0.1"),
        new LoginIP( "腾讯", "129.28.148.167"),
        new LoginIP( "局域网", "192.168.1.101"),
    };
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        var options = new List<Dropdown.OptionData>();
        foreach (var one in ipList)
        {
            options.Add(new Dropdown.OptionData(one.name + "   " + one.ip + ""));
        }
        var ips = GetComponent<Dropdown>();
        ips.options = options;
        if (defaultIpIndex < 0 || defaultIpIndex >= options.Count)
        {
            defaultIpIndex = 0;
        }
        ips.value = defaultIpIndex;
    }

    public void Dropdown_ipChange(int index)
    {
        defaultIpIndex = index;
    }

    public string GetLoginUrl()
    {
        return "http://" + ipList[defaultIpIndex].ip + ":3000";
    }

    class LoginIP
    {
        public string name;
        public string ip;

        public LoginIP(string _name, string _ip)
        {
            name = _name;
            ip = _ip;
        }
    }
}
