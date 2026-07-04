// K1Togami 2020/05/31

using UnityEngine;

public class AutoGenerateFloor : MonoBehaviour
{

   public int width = 30;
   public int height = 18;
   public float scale = 5.0f;

   [SerializeField] int mountainWidth = 10;
   [SerializeField] int mountainHeight = 10;


   [SerializeField] float wave = 0.1f;
   [SerializeField] float peak = 1.0f;

   const float TriangleHeight = 0.86660254f;
   const float TriangleHeightDouble = 1.7320508f;

   Mesh mesh;

   [ContextMenu("生成")]
   private void makeGround()
   {

       mesh = new Mesh();

       int p;

       mesh.Clear();


       var vertices = new Vector3[((width + 1) * 2 + 1) * (height + 1) + width + 1];
       var triangles = new int[(width * 2 + 1) * (height * 2 + 2) * 3];

       // メッシュ作成
       // 初段
       for (p = 0; p <= width; p++)
       {
           vertices[p].x = p * scale;
           vertices[p].z = 0f;
           vertices[p].y = mountainHeight;
       }
       for (int i = 0; i <= height; i++)
       {
           // 左端処理
           vertices[p].x = 0f;
           vertices[p].z = i * TriangleHeightDouble * scale + TriangleHeight * scale;
           vertices[p].y = mountainHeight;
           p++;

           for (int j = 0; (j <= width - 1); j++)
           {
               vertices[p].x = j * scale + 0.5f * scale;
               vertices[p].z = i * TriangleHeightDouble * scale + TriangleHeight * scale;
               vertices[p].y = GroundHeight(vertices[p].x, vertices[p].z);
               p++;
           }

           // 右端処理
           vertices[p].x = width * scale;
           vertices[p].z = i * TriangleHeightDouble * scale + TriangleHeight * scale;
           vertices[p].y = mountainHeight;
           p++;

           // 縦列はワンループで2つの三角形がペアです。
           for (int j = 0; j <= width; j++)
           {
               vertices[p].x = j * scale;
               vertices[p].z = (i + 1f) * TriangleHeightDouble * scale;
               vertices[p].y = GroundHeight(vertices[p].x, vertices[p].z);
               p++;
           }
       }

       p = 0;
       // メッシュ順作成
       for (int i = 0; i <= height; i++)
       {
           for (int j = 0; j < width; j++)
           {
               // 三角形4個を一組にして定義していきます。
               triangles[p + 0] = j + (((width + 1) * 2 + 1) * i);
               triangles[p + 1] = j + (width + 1) * (i * 2 + 1) + i;
               triangles[p + 2] = j + (width + 1) * (i * 2 + 1) + i + 1;

               triangles[p + 3] = j + (((width + 1) * 2 + 1) * i);  // 
               triangles[p + 4] = j + (width + 1) * (i * 2 + 1) + i + 1;
               triangles[p + 5] = j + (((width + 1) * 2 + 1) * i) + 1;

               triangles[p + 6] = j + (width + 1) * (i * 2 + 1) + i;
               triangles[p + 7] = j + (((width + 1) * 2 + 1) * (i + 1));
               triangles[p + 8] = j + (width + 1) * (i * 2 + 1) + i + 1;

               triangles[p + 9] = j + (width + 1) * (i * 2 + 1) + i + 1;
               triangles[p + 10] = j + (((width + 1) * 2 + 1) * (i + 1));
               triangles[p + 11] = j + (((width + 1) * 2 + 1) * (i + 1)) + 1;

               p += 12;
           }
           // 右恥と、左端処理
           triangles[p + 0] = width + (((width + 1) * 2 + 1) * i);
           triangles[p + 1] = width + (width + 1) * (i * 2 + 1) + i;
           triangles[p + 2] = width + (width + 1) * (i * 2 + 1) + i + 1;

           triangles[p + 3] = width + (width + 1) * (i * 2 + 1) + i;
           triangles[p + 4] = width + (((width + 1) * 2 + 1) * (i + 1));
           triangles[p + 5] = width + (width + 1) * (i * 2 + 1) + i + 1;

           p += 6;
       }

       mesh.vertices = vertices;
       mesh.triangles = triangles;

       mesh.RecalculateNormals();
       mesh.RecalculateBounds();
       var filter = GetComponent<MeshFilter>();
       filter.sharedMesh = mesh;
       var collider = GetComponent<MeshCollider>();
       collider.sharedMesh = null;
       collider.sharedMesh = mesh;
   }

    float GroundHeight(float x, float z)
{
    // ===== 通常の地形 =====
    float large = Mathf.PerlinNoise(x * wave * 0.03f, z * wave * 0.03f) * peak * 1.2f;
    float medium = Mathf.PerlinNoise(x * wave * 0.08f, z * wave * 0.08f) * peak * 0.5f;
    float small = Mathf.PerlinNoise(x * wave * 0.2f, z * wave * 0.2f) * peak * 0.2f;

    float baseHeight = large+small ;

    // ===== 中心座標 =====
    float centerX = width * scale * 0.5f;
    float centerZ = height * scale * 0.866f; // 三角グリッドなので少し補正

    // ===== 中心からの距離 =====
    float dist = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));

    // ===== 半径 =====
    float radius = mountainWidth * scale;

    // ===== 盆地マスク =====
    float mask = Mathf.Clamp01(dist / radius);
    //mask=Mathf.Pow(mask, 2f); // 中央を低くするために二乗する

    // なめらかにする
    mask = Mathf.SmoothStep(0f, 0.8f, mask);

    // ===== 山 =====
    float mountain = MountainHeight(x, z);

    // ===== 合成 =====
    float y = Mathf.Lerp(0f, baseHeight, 1 - mask)   // 中央は低く
            + Mathf.Lerp(0f, mountain, mask);        // 外側は高く

    return y;
}

    float MountainHeight(float x, float z)
    {
        float y = 0f;

        // 大きい地形（山）
        float large = Mathf.PerlinNoise(x * wave * 3, z * wave * 3f) * peak * 60f;

        // 中くらい（丘）
        //float medium = Mathf.PerlinNoise(x * wave , z * wave ) * peak * 5f;

        // 細かい凹凸
        //float small = Mathf.PerlinNoise(x * wave , z * wave  ) * peak * 2f;

        y = large ;

        // 山をなだらかにする
        //y = Mathf.Pow(y, 1.3f);

        return y;
    }
}