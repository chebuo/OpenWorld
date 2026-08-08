using UnityEngine;

public class DensityField
{
    public float[,,] data;
    Vector3Int dims;

    public DensityField(Vector3Int dims)
    {
        this.dims=dims;
        data =new float[dims.x,dims.y,dims.z];
    }

    public float Get(int x,int y,int z)=>data[x,y,z];

    public void Set(int x,int y,int z,float v)=>data[x,y,z]=v;
}