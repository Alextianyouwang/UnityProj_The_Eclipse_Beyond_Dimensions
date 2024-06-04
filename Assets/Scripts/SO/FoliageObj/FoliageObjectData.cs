using UnityEngine;

[CreateAssetMenu(menuName = "Foliage Object/Object Data")]
public class FoliageObjectData : ScriptableObject
{
    public Mesh[] SpawnMesh;
    public Material SpawnMeshMaterial;
    public Texture2D DensityMap;
    public int SquaredInstancePerTile = 3;
    public int SquaredChunksPerCluster = 4;
    public int SquaredTilePerClump = 8;
    [Range(0f, 4f)]
    public float OccludeeBoundScaleMultiplier = 1;
    [Range(0f, 1f)]
    public float DensityFilter = 1;
    public float DensityFalloffThreshold = 30;

    public bool UseMask = false;
    public bool ReverseMask = false;

}
