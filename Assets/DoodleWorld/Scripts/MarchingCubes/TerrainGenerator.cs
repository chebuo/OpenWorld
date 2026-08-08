using UnityEngine;
using MarchingCubes;

sealed class TerrainGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] Vector3Int _dimensions = new(128, 64, 128);
    [SerializeField] float _gridScale = 1f;
    [SerializeField] int _triangleBudget = 262144;
    [SerializeField] float _targetValue = 0;

    [Header("Basin & Mountain Wall Settings")]
    [Tooltip("盆地全体の半径 (メートル)。現在のDimensions (128x64x128) の範囲に合わせて初期値60mに設定しています。")]
    [SerializeField] float _basinRadius = 60.0f;

    [Tooltip("切り立った山壁が始まり、登れなくなる半径 (メートル)")]
    [SerializeField] float _wallStartRadius = 40.0f;

    [Tooltip("壁・山頂の高さ (メートル)")]
    [SerializeField] float _wallHeight = 35.0f;

    [Tooltip("壁の急鋭さ指数 (数値が大きいほど崖のように垂直に立ち上がる)")]
    [SerializeField] float _wallSteepness = 3.0f;

    [Header("Noise Settings")]
    [SerializeField] float _wave = 0.05f;
    [SerializeField] float _peak = 5.0f; 

    [Header("Compute Shaders")]
    [SerializeField] ComputeShader _densityCompute;
    [SerializeField] ComputeShader _builderCompute;

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;
    DensityField _density;

    int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

    void Start()
    {
        _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
        _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
        _density=new DensityField(_dimensions);

        GenerateCPU();
        UploadToGPU();
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
        _densityCompute.SetFloat("basinRadius", _basinRadius);
        _densityCompute.SetFloat("wallStartRadius", _wallStartRadius);
        _densityCompute.SetFloat("wallHeight", _wallHeight);
        _densityCompute.SetFloat("wallSteepness", _wallSteepness);
        _densityCompute.SetBuffer(0, "Voxels", _voxelBuffer);
        _densityCompute.DispatchThreads(0, _dimensions);
    }

    public void Dig(Vector3 worldPos, float radius)
    {
        // ⭐ ワールド → ローカル変換
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        int minX = Mathf.Max(0, (int)((localPos.x - radius) / _gridScale));
        int maxX = Mathf.Min(_dimensions.x, (int)((localPos.x + radius) / _gridScale));

        int minY = Mathf.Max(0, (int)((localPos.y - radius) / _gridScale));
        int maxY = Mathf.Min(_dimensions.y, (int)((localPos.y + radius) / _gridScale));

        int minZ = Mathf.Max(0, (int)((localPos.z - radius) / _gridScale));
        int maxZ = Mathf.Min(_dimensions.z, (int)((localPos.z + radius) / _gridScale));

        for (int x = minX; x < maxX; x++)
        for (int y = minY; y < maxY; y++)
        for (int z = minZ; z < maxZ; z++)
        {
            Vector3 pos = new Vector3(x, y, z) * _gridScale;

            float dist = Vector3.Distance(pos, localPos);

            if (dist < radius)
            {
                float v = _density.Get(x,y,z);
                _density.Set(x,y,z, v - (radius - dist));
            }
        }

        UploadToGPU();
        BuildMesh();
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

    void GenerateCPU()
    {
        for (int x = 0; x < _dimensions.x; x++)
        for (int y = 0; y < _dimensions.y; y++)
        for (int z = 0; z < _dimensions.z; z++)
        {
            float height = y;

            float noise = Mathf.PerlinNoise(x * _wave, z * _wave) * _peak;

            _density.Set(x,y,z, noise-height);
        }
    }

    void UploadToGPU()
    {
        float[] flat = new float[VoxelCount];

        int i = 0;
        for (int x = 0; x < _dimensions.x; x++)
        for (int y = 0; y < _dimensions.y; y++)
        for (int z = 0; z < _dimensions.z; z++)
            flat[i++] = _density.Get(x,y,z);

        _voxelBuffer.SetData(flat);
    }


}