using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
    [SerializeField] TMP_InputField cellNumberInput;
    [SerializeField] TMP_InputField velocityInput;
    [SerializeField] TMP_InputField turnRateInput;
    [SerializeField] TMP_InputField sensingDistanceInput;
    [SerializeField] TMP_InputField sensingAngleInput;

    Simulation simulation;
    SpeciesSettings speciesSettings;


    void Start()
    {
        GetReferences();
    }

    private void GetReferences()
    {
        simulation = FindObjectOfType<Simulation>();
        speciesSettings = simulation.GetComponent<SpeciesSettings>();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSimulation();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ResetSimulation();
        }
    }

    public void StartSimulation()
    {
        SetupSimulation();
        simulation.Run();
    }

    private void SetupSimulation()
    {
        SetNumberOfCells();
        SetTurnRate();
        SetVelocity();
        SetSensingAngle();
        SetSensingDistance();
    }

    public void ResetSimulation()
    {
        simulation.Reset();
    }

    private void SetNumberOfCells()
    {
        string input = cellNumberInput.text;
        uint number;

        if (UInt32.TryParse(input, out number))
        {
            simulation.numberOfCells = number;
        }
    }
    private void SetTurnRate()
    {
        string input = turnRateInput.text;
        float number;

        if (float.TryParse(input, out number))
        {
            speciesSettings.turnSpeed = number;
        }
    }

    private void SetVelocity()
    {
        string input = velocityInput.text;
        uint number;

        if (UInt32.TryParse(input, out number))
        {
            speciesSettings.velocity = number;
        }
    }

    private void SetSensingDistance()
    {
        string input = sensingDistanceInput.text;
        uint number;

        if (UInt32.TryParse(input, out number))
        {
            speciesSettings.senseDistance = number;
        }
    }

    private void SetSensingAngle()
    {
        string input = sensingAngleInput.text;
        float number;

        if (float.TryParse(input, out number))
        {
            float angle = number / (2 * Mathf.PI);
            speciesSettings.senseAngle = angle;
        }
    }


}
