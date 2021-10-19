using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace TerrainChunkSystem
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedTerrainMaterial
    {
        public TerrainMaterial a;
        public TerrainMaterial b;
        public TerrainMaterial c;
        public TerrainMaterial d;

        public PackedTerrainMaterial(TerrainMaterial material)
        {
            a = material;
            b = material;
            c = material;
            d = material;
        }

        public static PackedTerrainMaterial Select(PackedTerrainMaterial packedMaterialA, PackedTerrainMaterial packedMaterialB, bool4 selection)
        {
            return new()
            {
                a = selection.x ? packedMaterialB.a : packedMaterialA.a,
                b = selection.y ? packedMaterialB.b : packedMaterialA.b,
                c = selection.z ? packedMaterialB.c : packedMaterialA.c,
                d = selection.w ? packedMaterialB.d : packedMaterialA.d
            };
        }
    }
}