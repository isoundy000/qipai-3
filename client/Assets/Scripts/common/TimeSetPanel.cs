using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeSetPanel : MonoBehaviour
{
    private Text yearText;
    private Text monthText;
    private Text dayText;
    private Text hourText;
    private int nowYear;
    private int nowMonth;
    private int nowDay;
    private int nowHour;
    private Func<DateTime, bool> setTimeCb;
    void Awake()
    {
        yearText = transform.Find("year/Text").GetComponent<Text>();
        monthText = transform.Find("month/Text").GetComponent<Text>();
        dayText = transform.Find("day/Text").GetComponent<Text>();
        hourText = transform.Find("hour/Text").GetComponent<Text>();
        DateTime now = DateTime.Now;
        SetTime(now);

    }

    void SetTime(DateTime time)
    {
        nowYear = time.Year;
        nowMonth = time.Month;
        nowDay = time.Day;
        nowHour = time.Hour;
        yearText.text = time.Year.ToString();
        monthText.text = TwoFormat(time.Month);
        dayText.text = TwoFormat(time.Day);
        hourText.text = TwoFormat(time.Hour);
    }

    public void Init(Func<DateTime, bool> cb, string title, string timeStr)
    {
        setTimeCb = cb;
        if (title != "")
        {
            transform.Find("title").GetComponent<Text>().text = title;
        }
        if (timeStr == "")
        {
            return;
        }
        DateTime tmp;
        if (!DateTime.TryParse(timeStr, out tmp))
        {
            return;
        }
        SetTime(tmp);
    }

    public void Btn_add(int type)
    {
        if (type == 1)  // 年
        {
            if (nowYear >= 2099)
            {
                return;
            }
            nowYear += 1;
            yearText.text = nowYear.ToString();
            IfChangeDay();
        }
        else if (type == 2) // 月
        {
            nowMonth += 1;
            if (nowMonth > 12)
            {
                nowMonth = 1;
            }
            monthText.text = TwoFormat(nowMonth);
            IfChangeDay();
        }
        else if (type == 3) // 日
        {
            nowDay += 1;
            int maxDay = DateTime.DaysInMonth(nowYear, nowMonth);
            if (nowDay > maxDay)
            {
                nowDay = 1;
            }
            dayText.text = TwoFormat(nowDay);
        }
        else if (type == 4) // 时
        {
            nowHour += 1;
            if (nowHour >= 24)
            {
                nowHour = 0;
            }
            hourText.text = TwoFormat(nowHour);
        }
    }

    private void IfChangeDay()
    {
        int maxDay = DateTime.DaysInMonth(nowYear, nowMonth);
        if (nowDay > maxDay)
        {
            nowDay = maxDay;
            dayText.text = TwoFormat(nowDay);
        }
    }

    public void Btn_minus(int type)
    {
        if (type == 1)  // 年
        {
            if (nowYear <= 2000)
            {
                return;
            }
            nowYear -= 1;
            yearText.text = nowYear.ToString();
            IfChangeDay();
        }
        else if (type == 2) // 月
        {
            nowMonth -= 1;
            if (nowMonth == 0)
            {
                nowMonth = 12;
            }
            monthText.text = TwoFormat(nowMonth);
            IfChangeDay();
        }
        else if (type == 3) // 日
        {
            nowDay -= 1;
            if (nowDay == 0)
            {
                int maxDay = DateTime.DaysInMonth(nowYear, nowMonth);
                nowDay = maxDay;
            }
            dayText.text = TwoFormat(nowDay);
        }
        else if (type == 4) // 时
        {
            nowHour -= 1;
            if (nowHour < 0)
            {
                nowHour = 23;
            }
            hourText.text = TwoFormat(nowHour);
        }
    }

    public void Btn_yes()
    {
        if (setTimeCb == null)
        {
            Btn_close();
            return;
        }
        DateTime endTime = new DateTime(nowYear, nowMonth, nowDay, nowHour, 0, 0);
        bool res = setTimeCb(endTime);
        if (res)
        {
            setTimeCb = null;
            Btn_close();
        }

    }

    private string TwoFormat(int num)
    {
        return string.Format("{0:00}", num);
    }

    public void Btn_close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        setTimeCb = null;
    }
}

