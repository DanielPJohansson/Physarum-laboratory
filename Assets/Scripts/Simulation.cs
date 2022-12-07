using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField] ComputeShader simulationShader;
    [SerializeField] Vector2Int texResolution = new Vector2Int(512, 512);
    [SerializeField] Color backgroundColor;
    [SerializeField] public uint numberOfCells = 10;
    [SerializeField] Vector4 decayRate = new Vector4(1f, 0.5f, 0.2f, 0f);
    [SerializeField] Vector4 diffusionRate = new Vector4(0.2f, 0.5f, 2f, 0f);
    [SerializeField] Texture2D backgroundTexture;
    [SerializeField] Texture2D blackTexture;
    SpeciesSettings speciesSettings;

    Renderer rend;
    RenderTexture trailTexture;
    RenderTexture diffusionTexture;
    RenderTexture drawTexture;
    RenderTexture maskTexture;
    ComputeBuffer computeBuffer;
    int cellHandle;
    int diffuseHandle;

    uint threadGroupSizeX;
    int threads;

    public bool running = false;
    public bool useCircularShape = false;

    Cell[] cells;

    void Start()
    {
        GetReferences();
        CreateTextures();
        EnableRenderer();
        GetKernels();
    }

    void FixedUpdate()
    {
        if (running)
        {
            UpdateParameters();
            DispatchKernals();
        }
    }

    public void Run()
    {
        InitData();
        InitShader();
        running = true;
    }

    public void Reset()
    {
        running = false;
        if (computeBuffer is not null)
        {
            computeBuffer.Release();
            trailTexture.Release();
            diffusionTexture.Release();
            drawTexture.Release();
        }
    }

    void OnDestroy()
    {
        Reset();
    }

    private void GetReferences()
    {
        speciesSettings = GetComponent<SpeciesSettings>();
    }

    private void CreateTextures()
    {
        trailTexture = ShaderHelper.CreateTexture(texResolution);
        diffusionTexture = ShaderHelper.CreateTexture(texResolution);
        drawTexture = ShaderHelper.CreateTexture(texResolution);
        maskTexture = ShaderHelper.CreateTexture(texResolution);
    }

    private void EnableRenderer()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }
    private void GetKernels()
    {
        cellHandle = simulationShader.FindKernel("Cells");
        diffuseHandle = simulationShader.FindKernel("Diffuse");
        simulationShader.GetKernelThreadGroupSizes(cellHandle, out threadGroupSizeX, out _, out _);
    }

    private void InitData()
    {
        uint numberOfCellsToInstatiate = (numberOfCells / threadGroupSizeX) * threadGroupSizeX;
        List<Vector2> positions = new();

        if (useCircularShape)
        {
            positions = DataManager.GenerateRandomPointsInCircle(30, texResolution);
        }
        else
        {
            positions = DataManager.GenerateRandomPointsInTexture(30, texResolution);
        }

        cells = DataManager.GenerateCellsWithStartPositions(numberOfCellsToInstatiate, speciesSettings, positions);
    }

    private void InitShader()
    {
        GetThreadCount();
        SetTextures();
        SetParameters();
        SetBuffer();
    }


    private void GetThreadCount()
    {
        threads = Mathf.CeilToInt(((float)numberOfCells / (float)threadGroupSizeX));
    }

    private void SetTextures()
    {
        Graphics.Blit(backgroundTexture, maskTexture);
        simulationShader.SetTexture(cellHandle, "MaskTex", maskTexture);
        simulationShader.SetTexture(cellHandle, "TrailTex", trailTexture);
        simulationShader.SetTexture(cellHandle, "DrawTex", drawTexture);
        simulationShader.SetTexture(diffuseHandle, "DrawTex", drawTexture);
        simulationShader.SetTexture(diffuseHandle, "DiffusedTex", diffusionTexture);
        rend.material.SetTexture("_MainTex", drawTexture);
    }

    private void SetParameters()
    {
        simulationShader.SetVector("backgroundColor", backgroundColor);
        simulationShader.SetVector("cellColor", speciesSettings.cellColor);

        simulationShader.SetInt("texResolutionX", texResolution.x);
        simulationShader.SetInt("texResolutionY", texResolution.y);

        simulationShader.SetInt("centerPosX", texResolution.x / 2);
        simulationShader.SetInt("centerPosY", texResolution.y / 2);


        Debug.Log(speciesSettings.senseAngle);
        Debug.Log(speciesSettings.senseDistance);

        simulationShader.SetVector("decayRate", decayRate);
        simulationShader.SetVector("diffusionRate", diffusionRate);
        simulationShader.SetFloat("senseAngle", speciesSettings.senseAngle);
        simulationShader.SetFloat("turnSpeed", speciesSettings.turnSpeed);
        simulationShader.SetFloat("senseDistance", speciesSettings.senseDistance);
        simulationShader.SetBool("isCircular", useCircularShape);
    }
    private void SetBuffer()
    {
        int stride = (2 + 1 + 1) * sizeof(float);
        computeBuffer = new ComputeBuffer(cells.Length, stride);
        computeBuffer.SetData(cells);
        simulationShader.SetBuffer(cellHandle, "cellsBuffer", computeBuffer);
    }

    private void UpdateParameters()
    {
        simulationShader.SetFloat("time", Time.time);
        simulationShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        simulationShader.SetBool("sensing", !Input.GetKey(KeyCode.Return));
    }

    private void DispatchKernals()
    {
        simulationShader.Dispatch(cellHandle, threads, 1, 1);
        Graphics.Blit(trailTexture, drawTexture);

        simulationShader.Dispatch(diffuseHandle, texResolution.x / 8, texResolution.y / 8, 1);
        Graphics.Blit(diffusionTexture, trailTexture);
    }
}
