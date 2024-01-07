using System;
using System.Collections.Generic;
using Silk.NET.Maths;

namespace Magneplot.Generator.Curves
{
    public class SpiralSource : CurveSource
    {
        public double Radius { get; set; }
        public double Theta { get; set; }
        public double Step { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public uint Segments { get; set; }

        public override string Name
        {
            get
            {
                uint hashId = MathUtils.HashToUint(Radius, Theta, Step, MinY, MaxY, Segments);
                return "Spiral." + hashId;
            }
        }

        public SpiralSource(double radius, double theta, double step, double minY, double maxY, uint segments)
        {
            Radius = radius;
            Theta = theta;
            Step = step;
            MinY = minY;
            MaxY = maxY;
            Segments = segments;
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
