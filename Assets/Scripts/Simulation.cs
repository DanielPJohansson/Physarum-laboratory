using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] Vector2Int texResolution = new Vector2Int(512, 512);
    [SerializeField] Color cellColor;
    [SerializeField] Color backgroundColor;
    [SerializeField] uint numberOfCells = 10;
    [SerializeField] float velocity = 10f;
    [SerializeField] float velocityVariation = 10f;
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float senseAngle = 1f;
    [SerializeField] float senseDistance = 10f;
    [SerializeField] Vector4 decayRate = new Vector4(1f, 0.5f, 0.2f, 0f);
    [SerializeField] Vector4 diffusionRate = new Vector4(0.2f, 0.5f, 2f, 0f);
    [SerializeField] Texture2D maskTexture;

    Renderer rend;
    RenderTexture trailTexture;
    RenderTexture diffusionTexture;
    RenderTexture drawTexture;
    ComputeBuffer computeBuffer;
    int cellHandle;
    int diffuseHandle;

    uint threadGroupSizeX;
    int threads;

    struct Cell
    {
        public Vector2 position;
        public float angle;
        // public Vector2 direction;
        public float velocity;
    }

    Cell[] cells;

    void Start()
    {
        CreateTextures();

        EnableRenderer();

        InitData();

        InitShader();
    }

    void FixedUpdate()
    {
        UpdateParameters();
        DispatchKernals();
    }

    void OnDestroy()
    {
        computeBuffer.Release();
    }

    private void CreateTextures()
    {
        trailTexture = new RenderTexture(texResolution.x, texResolution.y, 0);
        trailTexture.enableRandomWrite = true;
        trailTexture.Create();

        diffusionTexture = new RenderTexture(texResolution.x, texResolution.y, 0);
        diffusionTexture.enableRandomWrite = true;
        diffusionTexture.Create();

        drawTexture = new RenderTexture(texResolution.x, texResolution.y, 0);
        drawTexture.enableRandomWrite = true;
        drawTexture.Create();
    }

    private void EnableRenderer()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }

    private void InitData()
    {
        uint numberOfCellsInstatiated = (numberOfCells / 64) * 64;
        cells = new Cell[numberOfCellsInstatiated];

        for (int i = 0; i < numberOfCellsInstatiated; i++)
        {
            Vector4 cellColor = new Vector4(1, 1, 1, 1);

            Cell cell = new()
            {
                position = new Vector2(texResolution.x / 2, texResolution.y / 2),
                angle = Random.Range(0f, 2 * Mathf.PI),
                velocity = Random.Range(velocity * (1 - velocityVariation), velocity * (1 + velocityVariation)),
            };
            cells[i] = cell;
        }
    }

    private void InitShader()
    {
        cellHandle = shader.FindKernel("Cells");
        diffuseHandle = shader.FindKernel("Diffuse");
        shader.GetKernelThreadGroupSizes(cellHandle, out threadGroupSizeX, out _, out _);
        threads = Mathf.CeilToInt(((float)numberOfCells / (float)threadGroupSizeX));

        SetParameters();
        SetTextures();

        int stride = (2 + 1 + 1) * sizeof(float);
        computeBuffer = new ComputeBuffer(cells.Length, stride);
        computeBuffer.SetData(cells);
        shader.SetBuffer(cellHandle, "cellsBuffer", computeBuffer);

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
        shader.SetVector("cellColor", cellColor);

        shader.SetInt("texResolutionX", texResolution.x);
        shader.SetInt("texResolutionY", texResolution.y);

        shader.SetVector("decayRate", decayRate);
        shader.SetVector("diffusionRate", diffusionRate);
        shader.SetFloat("senseAngle", senseAngle);
        shader.SetFloat("turnSpeed", turnSpeed);
        shader.SetFloat("senseDistance", senseDistance);
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
