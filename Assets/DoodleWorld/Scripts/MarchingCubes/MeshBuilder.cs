using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingCubes {

//
// Isosurface mesh builder with the marching cubes algorithm
//
sealed class MeshBuilder : System.IDisposable
{
    #region Public members

    public Mesh Mesh => _mesh;

    public Mesh ColliderMesh
    {
        get
        {
            UpdateColliderMesh();
            return _colliderMesh;
        }
    }

    public int ActiveTriangleCount
    {
        get
        {
            _countBuffer.GetData(_countArray);
            return Mathf.Min(_countArray[0], _triangleBudget);
        }
    }

    public MeshBuilder(int x, int y, int z, int budget, ComputeShader compute)
      => Initialize((x, y, z), budget, compute);

    public MeshBuilder(Vector3Int dims, int budget, ComputeShader compute)
      => Initialize((dims.x, dims.y, dims.z), budget, compute);

    public void Dispose()
      => ReleaseAll();

    public void BuildIsosurface(ComputeBuffer voxels, float target, float scale)
      => RunCompute(voxels, target, scale);

    public void UpdateMeshCollider(MeshCollider collider)
    {
        if (collider == null) return;

        int activeTriangles = ActiveTriangleCount;
        if (activeTriangles <= 0)
        {
            collider.sharedMesh = null;
            return;
        }

        UpdateColliderMesh();

        // PhysX の空間構造ツリー (BVH) を強制的に全領域で再構築
        if (_colliderMesh != null && _colliderMesh.vertexCount > 0)
        {
            Physics.BakeMesh(_colliderMesh.GetEntityId(), false);
        }

        collider.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning
                                | MeshColliderCookingOptions.WeldColocatedVertices
                                | MeshColliderCookingOptions.UseFastMidphase;
        collider.sharedMesh = null;
        collider.sharedMesh = _colliderMesh;
    }

    public void UpdateColliderMesh()
    {
        int activeTriangles = ActiveTriangleCount;
        if (activeTriangles <= 0)
        {
            if (_colliderMesh != null) _colliderMesh.Clear();
            return;
        }

        int activeVertices = activeTriangles * 3;
        EnsureColliderBuffers(activeVertices);

        _vertexBuffer.GetData(_vertexReadbackBuffer, 0, 0, activeVertices);

        var validPositions = new System.Collections.Generic.List<Vector3>(activeVertices);
        var validIndices = new System.Collections.Generic.List<int>(activeVertices);

        for (int i = 0; i < activeVertices; i += 3)
        {
            Vector3 v0 = _vertexReadbackBuffer[i + 0].position;
            Vector3 v1 = _vertexReadbackBuffer[i + 1].position;
            Vector3 v2 = _vertexReadbackBuffer[i + 2].position;

            // 1. 非有限値 (NaN, Infinity) チェック
            if (!float.IsFinite(v0.x) || !float.IsFinite(v0.y) || !float.IsFinite(v0.z) ||
                !float.IsFinite(v1.x) || !float.IsFinite(v1.y) || !float.IsFinite(v1.z) ||
                !float.IsFinite(v2.x) || !float.IsFinite(v2.y) || !float.IsFinite(v2.z)) continue;

            // 2. 完全に同じ座標の重なりチェック (縮退防止)
            if (v0 == v1 || v1 == v2 || v2 == v0) continue;

            // 3. 完全なゼロ面積チェック (直線上に並ぶ三角形を除外)
            Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
            if (cross == Vector3.zero) continue;

            // 4. 有効な正常ポリゴンとして登録
            int baseIdx = validPositions.Count;
            validPositions.Add(v0);
            validPositions.Add(v1);
            validPositions.Add(v2);

            validIndices.Add(baseIdx + 0);
            validIndices.Add(baseIdx + 1);
            validIndices.Add(baseIdx + 2);
        }

        _colliderMesh.Clear();

        if (validPositions.Count > 0)
        {
            _colliderMesh.SetVertices(validPositions);
            _colliderMesh.SetTriangles(validIndices, 0);
            _colliderMesh.RecalculateBounds();
            _colliderMesh.RecalculateNormals();
        }
    }

    #endregion

    #region Private members

    (int x, int y, int z) _grids;
    int _triangleBudget;
    ComputeShader _compute;

    void Initialize((int, int, int) dims, int budget, ComputeShader compute)
    {
        _grids = dims;
        _triangleBudget = budget;
        _compute = compute;

        AllocateBuffers();
        AllocateMesh(3 * _triangleBudget);
    }

    void ReleaseAll()
    {
        ReleaseBuffers();
        ReleaseMesh();
    }

    void RunCompute(ComputeBuffer voxels, float target, float scale)
    {
        _counterBuffer.SetCounterValue(0);

        // Isosurface reconstruction
        _compute.SetInts("Dims", _grids);
        _compute.SetInt("MaxTriangle", _triangleBudget);
        _compute.SetFloat("Scale", scale);
        _compute.SetFloat("Isovalue", target);
        _compute.SetBuffer(0, "TriangleTable", _triangleTable);
        _compute.SetBuffer(0, "Voxels", voxels);
        _compute.SetBuffer(0, "VertexBuffer", _vertexBuffer);
        _compute.SetBuffer(0, "IndexBuffer", _indexBuffer);
        _compute.SetBuffer(0, "Counter", _counterBuffer);
        _compute.DispatchThreads(0, _grids);

        // Copy active triangle count before ClearUnused runs
        ComputeBuffer.CopyCount(_counterBuffer, _countBuffer, 0);

        // Clear unused area of the buffers.
        _compute.SetBuffer(1, "VertexBuffer", _vertexBuffer);
        _compute.SetBuffer(1, "IndexBuffer", _indexBuffer);
        _compute.SetBuffer(1, "Counter", _counterBuffer);
        _compute.DispatchThreads(1, 1024, 1, 1);

        // Update active submesh descriptor for rendering
        int activeIndices = ActiveTriangleCount * 3;
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, activeIndices),
                         MeshUpdateFlags.DontRecalculateBounds);

        // Bounding box
        var ext = new Vector3(_grids.x, _grids.y, _grids.z) * scale;
        _mesh.bounds = new Bounds(Vector3.zero, ext);
    }

    #endregion

    #region Compute buffer objects

    ComputeBuffer _triangleTable;
    ComputeBuffer _counterBuffer;
    ComputeBuffer _countBuffer;
    int[] _countArray = new int[1];

    void AllocateBuffers()
    {
        // Marching cubes triangle table
        _triangleTable = new ComputeBuffer(256, sizeof(ulong));
        _triangleTable.SetData(PrecalculatedData.TriangleTable);

        // Buffer for triangle counting
        _counterBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
        _countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    void ReleaseBuffers()
    {
        _triangleTable.Dispose();
        _counterBuffer.Dispose();
        _countBuffer?.Dispose();
    }

    #endregion

    #region Mesh objects

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct VertexData
    {
        public Vector3 position;
        public Vector3 normal;
    }

    Mesh _mesh;
    GraphicsBuffer _vertexBuffer;
    GraphicsBuffer _indexBuffer;

    Mesh _colliderMesh;
    VertexData[] _vertexReadbackBuffer;
    Vector3[] _colliderPositions;
    int[] _colliderIndices;

    void EnsureColliderBuffers(int count)
    {
        if (_vertexReadbackBuffer == null || _vertexReadbackBuffer.Length < count)
        {
            _vertexReadbackBuffer = new VertexData[count];
            _colliderPositions = new Vector3[count];
        }

        if (_colliderIndices == null || _colliderIndices.Length < count)
        {
            int oldCount = _colliderIndices != null ? _colliderIndices.Length : 0;
            System.Array.Resize(ref _colliderIndices, count);
            for (int i = oldCount; i < count; i++)
            {
                _colliderIndices[i] = i;
            }
        }
    }

    void AllocateMesh(int vertexCount)
    {
        _mesh = new Mesh();
        _colliderMesh = new Mesh();
        _colliderMesh.name = "Marching Cubes Collider Mesh";

        // We want GraphicsBuffer access as Raw (ByteAddress) buffers.
        _mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        _mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

        // Vertex position: float32 x 3
        var vp = new VertexAttributeDescriptor
          (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        // Vertex normal: float32 x 3
        var vn = new VertexAttributeDescriptor
          (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        // Vertex/index buffer formats
        _mesh.SetVertexBufferParams(vertexCount, vp, vn);
        _mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

        // Submesh initialization
        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount),
                         MeshUpdateFlags.DontRecalculateBounds);

        // GraphicsBuffer references
        _vertexBuffer = _mesh.GetVertexBuffer(0);
        _indexBuffer = _mesh.GetIndexBuffer();
    }

    void ReleaseMesh()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        Object.Destroy(_mesh);
        if (_colliderMesh != null) Object.Destroy(_colliderMesh);
    }

    #endregion
}

} // namespace MarchingCubes
