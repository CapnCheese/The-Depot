using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    public float time;
    public TextMeshProUGUI[] timer;
    private int minutes;
    private int seconds;
    public GameObject wall;
    void Start()
    {
        time = 180;
    }
    void Update()
    {
        time -= Time.deltaTime;
        time = Mathf.Clamp(time, 0, 300);

        minutes = Mathf.FloorToInt(time / 60);
        seconds = Mathf.FloorToInt(time % 60);

        for (int i = 0; i < timer.Length; i++)
        {
            timer[i].text = $"TIME REMAINING: {minutes}:{seconds:D2}";
        }
        
        if (time <= 0 && wall != null)
        {
            Destroy(wall);
        }
    }
}