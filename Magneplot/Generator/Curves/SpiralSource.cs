using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Magneplot.Generator.Curves
{
    class SpiralSource : CurveSource
    {
        [JsonRequired]
        public double Radius { get; set; }

        public double Theta { get; set; }

        [JsonRequired]
        public double Step { get; set; }

        [JsonRequired]
        public double MinY { get; set; }

        [JsonRequired]
        public double MaxY { get; set; }

        [JsonRequired]
        public uint Segments { get; set; }

        public override string Name
        {
            get
            {
                uint hashId = MathUtils.HashToUint(Radius, Theta, Step, MinY, MaxY, Segments);
                return "Spiral." + hashId;
            }
        }

        public override List<Vector3D<double>> GetCurve()
        {
            double minT = MinY * MathUtils.TwoPi / Step;
            double maxT = MaxY * MathUtils.TwoPi / Step;

            List<Vector3D<double>> curve = new();
            for (uint i = 0; i <= Segments; i++)
            {
                double t = MathUtils.Lerp(minT, maxT, i / (double)Segments);
                curve.Add(new Vector3D<double>(
                    Math.Cos(t + Theta) * Radius,
                    t * Step / MathUtils.TwoPi,
                    Math.Sin(t + Theta) * Radius
                ));
            }

            return curve;
        }
    }
}
