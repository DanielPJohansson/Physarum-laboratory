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
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float senseAngle = 1f;
    [SerializeField] float senseDistance = 10f;
    [SerializeField] float decayRate = 1f;
    [SerializeField] float diffusionRate = 1f;

    Renderer rend;
    RenderTexture trailTexture;
    RenderTexture diffusionTexture;
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
        trailTexture = new RenderTexture(texResolution.x, texResolution.y, 0);
        trailTexture.enableRandomWrite = true;
        trailTexture.Create();
        diffusionTexture = new RenderTexture(texResolution.x, texResolution.y, 0);
        diffusionTexture.enableRandomWrite = true;
        diffusionTexture.Create();
        rend = GetComponent<Renderer>();
        rend.enabled = true;

        InitData();

        InitShader();
    }

    void FixedUpdate()
    {
        DispatchKernals();
    }

    private void InitData()
    {
        uint numberOfCellsInstatiated = (numberOfCells / 32) * 32;
        cells = new Cell[numberOfCellsInstatiated];

        for (int i = 0; i < numberOfCellsInstatiated; i++)
        {
            // int colorChoice = Random.Range(0, 2);
            Vector4 cellColor = new Vector4(1, 1, 1, 1);

            // if (colorChoice == 0)
            // {
            //     cellColor = new Vector4(1, 0, 0, 1);
            // }
            // else if (colorChoice == 1)
            // {
            //     cellColor = new Vector4(0, 1, 0, 1);
            // }
            // else if (colorChoice == 2)
            // {
            //     cellColor = new Vector4(0, 0, 1, 1);
            // }

            Cell cell = new()
            {
                position = new Vector2(texResolution.x / 2, texResolution.y / 2),
                angle = Random.Range(0f, 2 * Mathf.PI),
                // direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)),
                // velocity = Random.Range(velocity - velocity * 0.1f, velocity + velocity * 0.1f),
                velocity = velocity,
            };
            cells[i] = cell;
        }
    }

    private void InitShader()
    {
        cellHandle = shader.FindKernel("Cells");
        diffuseHandle = shader.FindKernel("Diffuse");
        Debug.Log(cellHandle);
        shader.GetKernelThreadGroupSizes(cellHandle, out threadGroupSizeX, out _, out _);
        threads = Mathf.CeilToInt(((float)numberOfCells / (float)threadGroupSizeX));

        Debug.Log(threadGroupSizeX);

        shader.SetVector("backgroundColor", backgroundColor);
        shader.SetVector("cellColor", cellColor);
        shader.SetInt("texResolutionX", texResolution.x);
        shader.SetInt("texResolutionY", texResolution.y);
        shader.SetFloat("deltaTime", Time.fixedDeltaTime);
        shader.SetFloat("decayRate", decayRate);
        shader.SetFloat("diffusionRate", diffusionRate);
        shader.SetFloat("senseAngle", senseAngle);
        shader.SetFloat("turnSpeed", turnSpeed);
        shader.SetFloat("senseDistance", senseDistance);

        shader.SetTexture(cellHandle, "TrailTex", trailTexture);
        shader.SetTexture(cellHandle, "DiffusedTex", diffusionTexture);
        shader.SetTexture(diffuseHandle, "TrailTex", trailTexture);
        shader.SetTexture(diffuseHandle, "DiffusedTex", diffusionTexture);

        int stride = (2 + 1 + 1) * sizeof(float);
        computeBuffer = new ComputeBuffer(cells.Length, stride);
        computeBuffer.SetData(cells);
        shader.SetBuffer(cellHandle, "cellsBuffer", computeBuffer);

        rend.material.SetTexture("_MainTex", diffusionTexture);
    }

    private void DispatchKernals()
    {
        Graphics.Blit(diffusionTexture, trailTexture);

        shader.SetFloat("time", Time.time);
        shader.SetFloat("deltaTime", Time.fixedDeltaTime);
        shader.Dispatch(cellHandle, threads, 1, 1);
    }


    void LateUpdate()
    {
        shader.Dispatch(diffuseHandle, texResolution.x / 8, texResolution.y / 8, 1);
    }
}
