using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{

    [SerializeField] TMP_InputField cellNumberInput;
    [SerializeField] TMP_InputField velocityInput;
    [SerializeField] TMP_InputField turnRateInput;
    [SerializeField] TMP_InputField sensingDistanceInput;
    [SerializeField] TMP_InputField sensingAngleInput;
    [SerializeField] TMP_Dropdown shapeSelector;

    public string Shape
    {
        get
        {
            int selected = shapeSelector.value;
            return shapeSelector.options[selected].text;
        }
    }
    public uint NumberOfCells
    {
        get
        {
            string input = cellNumberInput.text;
            uint number;

            UInt32.TryParse(input, out number);
            return number;
        }
    }
    public float TurnRate
    {
        get
        {
            string input = turnRateInput.text;
            float number;

            float.TryParse(input, out number);
            return number;
        }
    }
    public uint Velocity
    {
        get
        {
            string input = velocityInput.text;
            uint number;

            UInt32.TryParse(input, out number);
            return number;
        }
    }
    public uint SensingDistance
    {
        get
        {
            string input = sensingDistanceInput.text;
            uint number;

            UInt32.TryParse(input, out number);
            return number;
        }
    }
    public float SensingAngle
    {
        get
        {
            string input = sensingAngleInput.text;
            float number;

            float.TryParse(input, out number);
            Debug.Log("From UI: " + number);
            return number;
        }
    }

    public void SetValuesInUI(SpeciesSettings speciesSettings)
    {
        cellNumberInput.text = "10000";
        velocityInput.text = speciesSettings.velocity + "";
        turnRateInput.text = speciesSettings.turnSpeed + "";
        sensingDistanceInput.text = speciesSettings.senseDistance + "";
        sensingAngleInput.text = speciesSettings.senseAngle * 180 / MathF.PI + "";

    }

}
