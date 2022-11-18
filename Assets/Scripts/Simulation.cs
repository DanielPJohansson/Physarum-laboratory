using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] Vector2Int texResolution = new Vector2Int(512, 512);
    [SerializeField] Color backgroundColor;
    [SerializeField] public uint numberOfCells = 10;
    [SerializeField] Vector4 decayRate = new Vector4(1f, 0.5f, 0.2f, 0f);
    [SerializeField] Vector4 diffusionRate = new Vector4(0.2f, 0.5f, 2f, 0f);
    [SerializeField] Texture2D maskTexture;

    SpeciesSettings speciesSettings;

    Renderer rend;
    RenderTexture trailTexture;
    RenderTexture diffusionTexture;
    RenderTexture drawTexture;
    ComputeBuffer computeBuffer;
    int cellHandle;
    int diffuseHandle;

    uint threadGroupSizeX;
    int threads;

    public bool running = false;

    Cell[] cells;

    void Awake()
    {
        GetReferences();
    }

    void Start()
    {
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
    }

    private void EnableRenderer()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }
    private void GetKernels()
    {
        cellHandle = shader.FindKernel("Cells");
        diffuseHandle = shader.FindKernel("Diffuse");
        shader.GetKernelThreadGroupSizes(cellHandle, out threadGroupSizeX, out _, out _);
    }

    private void InitData()
    {
        uint numberOfCellsToInstatiate = (numberOfCells / threadGroupSizeX) * threadGroupSizeX;

        List<Vector2> positions = DataManager.GenerateRandomPointsInCircle(30, texResolution);
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
        shader.SetTexture(cellHandle, "TrailTex", trailTexture);
        shader.SetTexture(cellHandle, "DrawTex", drawTexture);
        shader.SetTexture(cellHandle, "MaskTex", maskTexture);
        shader.SetTexture(diffuseHandle, "DrawTex", drawTexture);
        shader.SetTexture(diffuseHandle, "DiffusedTex", diffusionTexture);
        rend.material.SetTexture("_MainTex", drawTexture);
    }

    private void SetParameters()
    {
        shader.SetVector("backgroundColor", backgroundColor);
        shader.SetVector("cellColor", speciesSettings.cellColor);

        shader.SetInt("texResolutionX", texResolution.x);
        shader.SetInt("texResolutionY", texResolution.y);

        shader.SetVector("decayRate", decayRate);
        shader.SetVector("diffusionRate", diffusionRate);
        shader.SetFloat("senseAngle", speciesSettings.senseAngle);
        shader.SetFloat("turnSpeed", speciesSettings.turnSpeed);
        shader.SetFloat("senseDistance", speciesSettings.senseDistance);
    }
    private void SetBuffer()
    {
        int stride = (2 + 1 + 1) * sizeof(float);
        computeBuffer = new ComputeBuffer(cells.Length, stride);
        computeBuffer.SetData(cells);
        shader.SetBuffer(cellHandle, "cellsBuffer", computeBuffer);
    }

    private void UpdateParameters()
    {
        shader.SetFloat("time", Time.time);
        shader.SetFloat("deltaTime", Time.fixedDeltaTime);
        shader.SetBool("sensing", !Input.GetKey(KeyCode.Return));
    }

    private void DispatchKernals()
    {
        shader.Dispatch(cellHandle, threads, 1, 1);
        Graphics.Blit(trailTexture, drawTexture);

        shader.Dispatch(diffuseHandle, texResolution.x / 8, texResolution.y / 8, 1);
        Graphics.Blit(diffusionTexture, trailTexture);
    }
}
