using System;
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
        
        public TerrainMaterial e;
        public TerrainMaterial f;
        public TerrainMaterial g;
        public TerrainMaterial h;

        public PackedTerrainMaterial(TerrainMaterial material)
        {
            a = material;
            b = material;
            c = material;
            d = material;
            e = material;
            f = material;
            g = material;
            h = material;
        }

        public static PackedTerrainMaterial Select(PackedTerrainMaterial packedMaterialA, PackedTerrainMaterial packedMaterialB, bool8 selection)
        {
            return new()
            {
                a = selection.a.x ? packedMaterialB.a : packedMaterialA.a,
                b = selection.a.y ? packedMaterialB.b : packedMaterialA.b,
                c = selection.a.z ? packedMaterialB.c : packedMaterialA.c,
                d = selection.a.w ? packedMaterialB.d : packedMaterialA.d,
                e = selection.b.x ? packedMaterialB.e : packedMaterialA.e,
                f = selection.b.y ? packedMaterialB.f : packedMaterialA.f,
                g = selection.b.z ? packedMaterialB.g : packedMaterialA.g,
                h = selection.b.w ? packedMaterialB.h : packedMaterialA.h,
            };
        }
        
        unsafe public TerrainMaterial this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 4)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (TerrainMaterial* array = &a) { return ((TerrainMaterial*)array)[index]; }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 4)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (TerrainMaterial* array = &a) { array[index] = value; }
            }
        }
        
        //todo optiimize for simd
        public static PackedTerrainMaterial Lerp(PackedTerrainMaterial packedMaterialA,
            PackedTerrainMaterial packedMaterialB, PackedFloat t)
        {
            throw new NotImplementedException();
            // t = SimdMath.clamp(t, 0, 1);
            //
            // bool4 bGreater = t.PackedValues > 0.5f;
            //
            // var result = Select(packedMaterialA, packedMaterialB, bGreater);
            //
            // result.a.SetColor(math.lerp(packedMaterialA.a.GetColor, packedMaterialB.a.GetColor, t.PackedValues[0]));
            // result.b.SetColor(math.lerp(packedMaterialA.b.GetColor, packedMaterialB.b.GetColor, t.PackedValues[1]));
            // result.c.SetColor(math.lerp(packedMaterialA.c.GetColor, packedMaterialB.c.GetColor, t.PackedValues[2]));
            // result.d.SetColor(math.lerp(packedMaterialA.d.GetColor, packedMaterialB.d.GetColor, t.PackedValues[3]));
            // return result;
        }
    }
}