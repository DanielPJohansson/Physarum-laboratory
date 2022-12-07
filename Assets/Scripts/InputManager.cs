using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputManager : MonoBehaviour
{
    SimulationManager simulationManager;

    void Start()
    {
        GetReferences();
    }

    private void GetReferences()
    {
        simulationManager = GetComponent<SimulationManager>();
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
        simulationManager.StartSimulation();
    }

    public void ResetSimulation()
    {
        simulationManager.ResetSimulation();
    }
}
