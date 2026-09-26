using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [SerializeField]int _itemCount;
    [SerializeField]int _minDepth;
    [SerializeField]int _maxDepth;


    [SerializeField]TerrainGenerator terrainGenerator;
    [SerializeField]ItemData itemData;
    bool SpawnRandomItem()
    {
        Vector3Int dims = terrainGenerator.Dimensions;

        int x = Random.Range(2, dims.x - 2);
        int y = Random.Range(_minDepth, _maxDepth);
        int z = Random.Range(2, dims.z - 2);

        Vector3 localPos =terrainGenerator.GetVoxelLocalPosition(x, y, z);

        int itemIndex=Random.Range(0, itemData.items.Length);
        ItemParams item=itemData.items[itemIndex];

        GameObject itemObj=Instantiate(item.prefab,terrainGenerator.transform.TransformPoint(localPos),Quaternion.identity,transform);
        itemObj.GetComponent<ItemController>().Init(item);

        return true;
    }

    public void GenerateItem()
    {
        int spawned=0;
        for (int i = 0; i < _itemCount; i++)
        {
            if(spawned>=_itemCount)break;
            if(SpawnRandomItem())spawned++;
        }
    }
}
