using System;
using henningboat.CubeMarching.Runtime.GeometrySystems.GeometryFieldSetup;

namespace henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem
{
    public struct DistanceDataReadbackCollection
    {
        private GeometryFieldData _data0;
        private GeometryFieldData _data1;
        private GeometryFieldData _data2;
        private GeometryFieldData _data3;
        private GeometryFieldData _data4;
        private GeometryFieldData _data5;
        private GeometryFieldData _data6;
        private GeometryFieldData _data7;

        public int Capacity => 8;

        public GeometryFieldData this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _data0;
                    case 1: return _data1;
                    case 2: return _data2;
                    case 3: return _data3;
                    case 4: return _data4;
                    case 5: return _data5;
                    case 6: return _data6;
                    case 7: return _data7;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _data0 = value;
                        break;
                    case 1:
                        _data1 = value;
                        break;
                    case 2:
                        _data2 = value;
                        break;
                    case 3:
                        _data3 = value;
                        break;
                    case 4:
                        _data4 = value;
                        break;
                    case 5:
                        _data5 = value;
                        break;
                    case 6:
                        _data6 = value;
                        break;
                    case 7:
                        _data7 = value;
                        break;
                }
            }
        }
    }
}