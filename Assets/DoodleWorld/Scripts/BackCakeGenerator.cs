using UnityEngine;

public class BackCakeGeneraotr : MonoBehaviour
{
    Vector3Int size=new Vector3Int(64,64,64);
    [SerializeField]Material material;

    void Start()
    {
        CreateCake(size);
    }

    void CreateCake(Vector3 size)
    {
        Mesh mesh=new Mesh();
        mesh.name="backCake";

        Vector3 half=size*0.35f;

        Vector3[] vertices =
        {
            new Vector3(-half.x, -half.y, -half.z), // 0
            new Vector3( half.x, -half.y, -half.z), // 1
            new Vector3( half.x,  half.y, -half.z), // 2
            new Vector3(-half.x,  half.y, -half.z), // 3
            new Vector3(-half.x, -half.y,  half.z), // 4
            new Vector3( half.x, -half.y,  half.z), // 5
            new Vector3( half.x,  half.y,  half.z), // 6
            new Vector3(-half.x,  half.y,  half.z)  // 7
        };

        int[] triangles =
        {
            // 前面
            4, 5, 6, 4, 6, 7,

            // 背面
            1, 0, 3, 1, 3, 2,

            // 左面
            0, 4, 7, 0, 7, 3,

            // 右面
            5, 1, 2, 5, 2, 6,

            // 上面
            3, 7, 6, 3, 6, 2,

            // 下面
            0, 1, 5, 0, 5, 4
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // 法線を計算
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // GameObjectを作成
        GameObject cube = new GameObject("CustomCube");

        MeshFilter meshFilter = cube.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cube.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = material;

        // このスクリプトが付いたオブジェクトの子にする
        cube.transform.SetParent(transform, false);
    }
}