using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Magneplot.Generator.Curves
{
    class LineSource : CurveSource
    {
        [JsonRequired]
        public Vector3D<double> From { get; set; }

        [JsonRequired]
        public Vector3D<double> To { get; set; }

        [JsonRequired]
        public uint Segments { get; set; }

        public override string Name
        {
            get
            {
                uint hashId = MathUtils.HashToUint(From.X, From.Y, From.Z, To.X, To.Y, To.Z);
                return "Line." + hashId;
            }
        }

        public override List<Vector3D<double>> GetCurve()
        {
            List<Vector3D<double>> curve = [];
            for (uint i = 0; i <= Segments; i++)
            {
                curve.Add(Vector3D.Lerp(From, To, i / (double)Segments));
            }

            return curve;
        }
    }
}
