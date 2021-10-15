using TerrainChunkSystem;
using Unity.Entities;
using UnityEngine;

//todo convert into more general TerrainShaper component?
public struct CTerrainMaterial : IComponentData
{
    #region Public Fields

    [HideInInspector] public PackedTerrainMaterial Material;

    #endregion
}