using UnityEngine;
using MarchingCubes;

sealed class TerrainGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] Vector3Int _dimensions = new(128, 64, 128);
    [SerializeField] float _gridScale = 1f;
    [SerializeField] int _triangleBudget = 262144;
    [SerializeField] float _targetValue = 0;

    [Header("Compute Shaders")]
    [SerializeField] ComputeShader _densityCompute;
    [SerializeField] ComputeShader _builderCompute;

    ComputeBuffer _voxelBuffer;
    MeshBuilder _builder;
    DensityField _density;

    int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

    public bool isInit=false;

    void Start()
    {
        _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
        _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
        _density = new DensityField(_dimensions);

        // 1. ComputeShader で地形密度を生成
        GenerateGPU();

        // 2. GPU のボクセルデータを CPU 側の _density に読み込んで同期
        DownloadFromGPU();

        // 3. メッシュ構築
        BuildMesh();

        isInit=true;
    }

    void GenerateGPU()
    {
        _densityCompute.SetInts("Dims", _dimensions);
        _densityCompute.SetFloat("scale", _gridScale);
        _densityCompute.SetBuffer(0, "Voxels", _voxelBuffer);
        _densityCompute.DispatchThreads(0, _dimensions);
    }

    // 地形を掘る処理
    public void Dig(Vector3 worldPos, float radius)
    {
        // ワールド座標 → オブジェクトのローカル座標へ変換
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Marching Cubes のローカル原点は (Dims / 2) なので、グリッド空間上の中心座標を取得
        Vector3 gridCenter = (localPos / _gridScale) + ((Vector3)_dimensions * 0.5f);
        float gridRadius = radius / _gridScale;

        int margin = 2; // 掘削範囲の余白 (ボクセルの半分程度)

        // 影響範囲のボクセルインデックス範囲を計算
        int minX = Mathf.Clamp(Mathf.FloorToInt(gridCenter.x - gridRadius - margin), 0, _dimensions.x-margin);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(gridCenter.x + gridRadius + margin), 0, _dimensions.x-margin);

        int minY = Mathf.Clamp(Mathf.FloorToInt(gridCenter.y - gridRadius - margin), 0, _dimensions.y-margin);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(gridCenter.y + gridRadius + margin), 0, _dimensions.y-margin);

        int minZ = Mathf.Clamp(Mathf.FloorToInt(gridCenter.z - gridRadius - margin), 0, _dimensions.z-margin);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt(gridCenter.z + gridRadius + margin), 0, _dimensions.z-margin);

        bool modified = false;

        for (int x = minX; x < maxX; x++)
        for (int y = minY; y < maxY; y++)
        for (int z = minZ; z < maxZ; z++)
        {
            if(x<margin||x>=_dimensions.x-margin||y<margin||y>=_dimensions.y-margin||z<margin||z>=_dimensions.z-margin)continue;
            // 各ボクセルのローカル空間座標
            Vector3 voxelLocalPos = (new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) - (Vector3)_dimensions * 0.5f) * _gridScale;

            // 掘削ポイントからの距離
            float dist = Vector3.Distance(voxelLocalPos, localPos);

            if (dist < radius)
            {
                float currentDensity = _density.Get(x, y, z);
                float targetAirDensity =  currentDensity - (radius - dist);
                float newDensity = Mathf.Min(currentDensity, targetAirDensity);

                if (currentDensity != newDensity)
                {
                    _density.Set(x, y, z, newDensity);
                    modified = true;
                }
            }
        }

        if (modified)
        {
            // 変更されたボクセルデータを GPU へ転送してメッシュを即座に再構築
            UploadToGPU();
            BuildMesh();
        }
    }

    void BuildMesh()
    {
        _builder.BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);
        GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
        _builder.UpdateMeshCollider(GetComponent<MeshCollider>());
    }

    void UploadToGPU()
    {
        float[] flat = new float[VoxelCount];

        for (int z = 0; z < _dimensions.z; z++)
        for (int y = 0; y < _dimensions.y; y++)
        for (int x = 0; x < _dimensions.x; x++)
        {
            int index = x + _dimensions.x * (y + _dimensions.y * z);
            flat[index] = _density.Get(x, y, z);
        }

        _voxelBuffer.SetData(flat);
    }

    void DownloadFromGPU()
    {
        float[] flat = new float[VoxelCount];
        _voxelBuffer.GetData(flat);

        for (int z = 0; z < _dimensions.z; z++)
        for (int y = 0; y < _dimensions.y; y++)
        for (int x = 0; x < _dimensions.x; x++)
        {
            int index = x + _dimensions.x * (y + _dimensions.y * z);
            _density.Set(x, y, z, flat[index]);
        }
    }

    void OnDestroy()
    {
        _voxelBuffer.Dispose();
        _builder.Dispose();
    }
}