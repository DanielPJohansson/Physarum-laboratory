using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{

    Simulation simulation;
    SpeciesSettings speciesSettings;
    UIManager ui;

    void Start()
    {
        GetReferences();
        ui.SetValuesInUI(speciesSettings);
    }

    private void GetReferences()
    {
        simulation = FindObjectOfType<Simulation>();
        speciesSettings = simulation.GetComponent<SpeciesSettings>();
        ui = GetComponent<UIManager>();
    }


    public void StartSimulation()
    {
        SetSimulationParameters();
        simulation.Run();
    }

    private void SetSimulationParameters()
    {
        SetNumberOfCells();
        SetTurnRate();
        SetVelocity();
        SetSensingAngle();
        SetSensingDistance();
        SetShape();
    }

    public void ResetSimulation()
    {
        simulation.Reset();
    }

    private void SetShape()
    {
        if (ui.Shape == "Square")
        {
            simulation.useCircularShape = false;
        }
        else if (ui.Shape == "Circle")
        {
            simulation.useCircularShape = true;
        }
    }

    private void SetNumberOfCells()
    {
        simulation.numberOfCells = ui.NumberOfCells;
    }

    private void SetTurnRate()
    {
        speciesSettings.turnSpeed = ui.TurnRate;
    }

    private void SetVelocity()
    {
        speciesSettings.velocity = ui.Velocity;
    }

    private void SetSensingDistance()
    {
        speciesSettings.senseDistance = ui.SensingDistance;
    }

    private void SetSensingAngle()
    {
        float angle = ui.SensingAngle * Mathf.PI / 180;
        Debug.Log("After conversion: " + angle);
        speciesSettings.senseAngle = angle;
    }
}
