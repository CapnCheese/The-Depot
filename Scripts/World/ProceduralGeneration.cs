using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public class ProceduralGeneration : MonoBehaviour
{
    [Header("Settings")]
    public int maxGrass;
    public int maxFences;
    public Vector2 spawningArea;
    public GameObject fence;
    public Mesh grassMesh;
    public Material grassMaterial;
    public LayerMask ground;
    private RaycastHit groundHit;
    public float grassSize;
    public float raycastHeight;
    [Header("Rendering")]
    public ComputeShader cullShader;
    public float minDrawDistance = 5;
    public float maxDrawDistance;
    private ComputeBuffer transformBuffer;
    private ComputeBuffer culledTransformBuffer;
    private ComputeBuffer argsBuffer;
    private Bounds renderBounds;
    private int instanceCount;
    private int kernelID;
    private Camera cam;
    public bool PlaceGrass;
    struct GrassData
    {
        public Matrix4x4 transform;
    }
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Infinite") return;

        for (int i = 0; i < maxFences; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-spawningArea.x, 
            spawningArea.x), raycastHeight, 
            Random.Range(-spawningArea.y, spawningArea.y));
            
            if(Physics.Raycast(pos, Vector3.down, out RaycastHit fenceGroundHit, 150, ground))
            {
                Vector3 worldPos = fenceGroundHit.point - (Vector3.down * 0.25f);
                Quaternion rot = Quaternion.LookRotation(groundHit.normal);
                GameObject newFence = Instantiate(fence, worldPos, rot);
                newFence.transform.parent = transform;
                newFence.transform.rotation = Quaternion.Euler(
                    transform.rotation.x,
                     transform.rotation.y + Random.Range(-45, 45),
                     transform.rotation.z);
            }
        }

        if(!PlaceGrass) return;

        cam = FindFirstObjectByType<Camera>();

        List<GrassData> grassDataList = new List<GrassData>();
        for (int i = 0; i < maxGrass; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-spawningArea.x, 
            spawningArea.x), raycastHeight, 
            Random.Range(-spawningArea.y, spawningArea.y));
            
            if(Physics.Raycast(pos, Vector3.down, out groundHit, 150, ground))
            {
            Vector3 worldPos = new Vector3(pos.x, groundHit.point.y+grassSize, pos.z);
            Quaternion rot = Quaternion.Euler(0, Random.Range(0,360), 0);
            
            grassDataList.Add(new GrassData{
                transform = Matrix4x4.TRS(worldPos, rot, 
                new Vector3(grassSize, grassSize, grassSize))});

            Debug.DrawRay(worldPos, Vector3.up * 2, Color.purple, 10f);
            }
        }
        instanceCount = grassDataList.Count;
        SetupBuffers(grassDataList);
        Debug.Log("Grass placed: " + instanceCount);
    }
    void SetupBuffers(List<GrassData> grassDataList)
    {
        //transform buffer - one matrix per grass
        transformBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16);
        transformBuffer.SetData(grassDataList);

        culledTransformBuffer = new ComputeBuffer(
            instanceCount, sizeof(float) * 16, ComputeBufferType.Append);
        
        grassMaterial.SetBuffer("_TransformBuffer", culledTransformBuffer);

        argsBuffer = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        uint[] args = new uint[5];
        args[0] = grassMesh.GetIndexCount(0);
        args[1] = 0;
        args[2] = grassMesh.GetIndexStart(0);
        args[3] = grassMesh.GetBaseVertex(0);
        args[4] = 0;
        argsBuffer.SetData(args);
        
        kernelID = cullShader.FindKernel("CSMain");
        
        renderBounds = new Bounds(transform.position, 
            new Vector3(spawningArea.x * 2, 200, spawningArea.y * 2));
    }

    void Update()
    {
        if(!PlaceGrass) return;
        if(SceneManager.GetActiveScene().name == "Infinite") return;
        if (argsBuffer == null) return;

        culledTransformBuffer.SetCounterValue(0);
        cullShader.SetBuffer(kernelID, "_InputTransforms", transformBuffer);
        cullShader.SetBuffer(kernelID, "_OutputTransforms", culledTransformBuffer);
        cullShader.SetVector("_CameraPos", cam.transform.position);
        cullShader.SetFloat("_MinDistance", minDrawDistance);
        cullShader.SetFloat("_MaxDistance", maxDrawDistance);
        cullShader.SetInt("_InstanceCount", instanceCount);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Vector4[] frustumPlanes = new Vector4[6];
        for (int i = 0; i < 6; i++)
            frustumPlanes[i] = new Vector4(planes[i].normal.x, planes[i].normal.y,
                planes[i].normal.z, planes[i].distance);
        cullShader.SetVectorArray("_FrustumPlanes", frustumPlanes);

        int threadGroups = Mathf.CeilToInt(instanceCount / 64f);
        cullShader.Dispatch(kernelID, threadGroups, 1, 1);

        ComputeBuffer.CopyCount(culledTransformBuffer, argsBuffer, sizeof(uint));

        grassMaterial.SetBuffer("_TransformBuffer", culledTransformBuffer);

        UnityEngine.Graphics.DrawMeshInstancedIndirect(
            grassMesh, 0, grassMaterial, renderBounds, argsBuffer);
    }
    void OnDisable()
    {
        transformBuffer?.Release();
        culledTransformBuffer?.Release();
        argsBuffer?.Release();
    }
}