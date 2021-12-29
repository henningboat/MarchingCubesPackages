using System.Runtime.InteropServices;
using SIMDMath;
using Unity.Mathematics;

namespace henningboat.CubeMarching.Runtime.TerrainChunkSystem
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
        
        //todo optiimize for simd
        public static PackedTerrainMaterial Lerp(PackedTerrainMaterial packedMaterialA,
            PackedTerrainMaterial packedMaterialB, PackedFloat t)
        {
            t = SimdMath.clamp(t, 0, 1);

            bool4 bGreater = t.PackedValues > 0.5f;

            var result = Select(packedMaterialA, packedMaterialB, bGreater);

            result.a.SetColor(math.lerp(packedMaterialA.a.GetColor, packedMaterialB.a.GetColor, t.PackedValues[0]));
            result.b.SetColor(math.lerp(packedMaterialA.b.GetColor, packedMaterialB.b.GetColor, t.PackedValues[1]));
            result.c.SetColor(math.lerp(packedMaterialA.c.GetColor, packedMaterialB.c.GetColor, t.PackedValues[2]));
            result.d.SetColor(math.lerp(packedMaterialA.d.GetColor, packedMaterialB.d.GetColor, t.PackedValues[3]));
            return result;
        }
    }
}