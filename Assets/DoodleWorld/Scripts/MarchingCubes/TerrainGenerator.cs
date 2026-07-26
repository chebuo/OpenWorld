using UnityEngine;
using MarchingCubes;

sealed class TerrainGenerator : MonoBehaviour
{
    [SerializeField] Vector3Int _dimensions = new(64,32,64);

    [SerializeField] float _width=160;
    [SerializeField] float _height=80;
    [SerializeField] float _gridScale = 1f;

    [SerializeField] int _mountainWidth = 400;
    [SerializeField] int _mountainHeight = 200;


    [SerializeField] float _wave = 0.001f;
    [SerializeField] float _peak = 400.0f; 

    [SerializeField] int _triangleBudget = 65536;
    [SerializeField] float _targetValue = 0;

    [SerializeField] ComputeShader _densityCompute;
    [SerializeField] ComputeShader _builderCompute;

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;

    int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

    void Start()
    {
        _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
        _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);

        Generate();
        BuildMesh();
    }

    void Generate()
    {
        _densityCompute.SetInts("Dims", _dimensions);
        _densityCompute.SetFloat("scale", _gridScale);
        _densityCompute.SetFloat("wave", _wave);
        _densityCompute.SetFloat("peak", _peak);
        _densityCompute.SetFloat("width", _dimensions.x);
        _densityCompute.SetFloat("height", _dimensions.z);
        _densityCompute.SetFloat("mountainWidth", _mountainWidth);
        _densityCompute.SetBuffer(0, "Voxels", _voxelBuffer);
        _densityCompute.DispatchThreads(0, _dimensions);
    }

    void BuildMesh()
    {
        _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
        _builder.UpdateMeshCollider(GetComponent<MeshCollider>());
    }

    void OnDestroy()
    {
        _voxelBuffer.Dispose();
        _builder.Dispose();
    }
}