using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
    [SerializeField] TMP_InputField cellNumberInput;

    Simulation simulation;


    void Start()
    {
        GetReferences();
    }

    private void GetReferences()
    {
        simulation = FindObjectOfType<Simulation>().GetComponent<Simulation>();
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
    }

    public void ResetSimulation()
    {
        simulation.Reset();
    }

    public void SetNumberOfCells()
    {
        string input = cellNumberInput.text;
        uint number;

        if (UInt32.TryParse(input, out number))
        {
            simulation.numberOfCells = number;
        }
    }


}
