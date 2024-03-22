using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class StationControl : MonoBehaviour
{
    public TMP_Text powerText;
    public TMP_Text heatingText;
    public TMP_Text setText;
    public TMP_Text currentText;
    public StationButton powerButton;
    public StationButton heatingButton;
    public StationButton setUpButton;
    public StationButton setDownButton;
    public SolderingIron iron;
    private string degree = "°C";
    public bool heating { get; private set;}
    public bool power { get; private set; }
    private float minTemp;
    private void Start()
    {
        powerText.text = "Power: OFF";
        heatingText.text = "Heating: OFF";
        minTemp = 150;
        setText.text = "Set: "+minTemp.ToString()+degree;
        currentText.text = "Current: " + SceneManager.instance.roomTemperature.ToString()+degree;
    }
    private void Update()
    {
        if (powerButton.on)
        {
            power = true;
            powerText.text = "Power: On";
        }
        else
        {
            power = false;
            powerText.text = "Power: OFF";
            heatingButton.on = false;
            setUpButton.count = 0;
            setDownButton.count = 0;
        }
        iron.on = heatingButton.on;
        if(heatingButton.on)
        {
            heatingText.text = "Heating: On";
        }
        else
        {
            heatingText.text = "Heating: OFF";
        }
        float newTemp = minTemp + 10*setUpButton.count - 10*setDownButton.count;
        if(newTemp >= minTemp)
        {
            iron.setTemperature = newTemp;
            setText.text = "Set: " + newTemp.ToString() + degree;
        }
        else
        {
            iron.setTemperature = minTemp;
            setText.text = "Set: " + minTemp.ToString() + degree;
        }

        currentText.text = "Current: " + Mathf.Round(iron.temperature) + degree;
    }
}
