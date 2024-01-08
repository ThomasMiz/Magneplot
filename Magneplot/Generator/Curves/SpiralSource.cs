using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Magneplot.Generator.Curves
{
    class SpiralSource : CurveSource
    {
        public Vector3D<double> Center { get; set; } = Vector3D<double>.Zero;

        [JsonRequired]
        public Vector3D<double> Direction { get; set; }

        public Vector3D<double> StartTowards { get; set; } = Vector3D<double>.Zero;

        [JsonRequired]
        public double Radius { get; set; }

        [JsonRequired]
        public double Length { get; set; }

        [JsonRequired]
        public double Step { get; set; }

        [JsonRequired]
        public uint Segments { get; set; }

        public override string Name
        {
            get
            {
                NormalizeVectors();
                uint hashId = MathUtils.HashToUint(
                    Center.X, Center.Y, Center.Z, Direction.X, Direction.Y, Direction.Z,
                    StartTowards.X, StartTowards.Y, StartTowards.Z, Radius, Length, Step, Segments
                );

                return "Spiral." + hashId;
            }
        }

        public void NormalizeVectors()
        {
            Direction = Vector3D.Normalize(Direction);

            if (StartTowards == default)
            {
                // Takes the unit vector that is the least parallel to the direction vector, then makes it
                // normal to it by applying Gram–Schmidt.
                (Vector3D<double>, double)[] arr = [
                    (Vector3D<double>.UnitX, Vector3D.Dot(Vector3D<double>.UnitX, Direction)),
                    (Vector3D<double>.UnitY, Vector3D.Dot(Vector3D<double>.UnitY, Direction)),
                    (Vector3D<double>.UnitZ, Vector3D.Dot(Vector3D<double>.UnitZ, Direction))
                ];

                (Vector3D<double>, double) val = arr.MinBy(x => Math.Abs(x.Item2));
                StartTowards = val.Item1 - val.Item2 * Direction;
            }
            else
            {
                double d = Vector3D.Dot(StartTowards, Direction);
                if (Math.Abs(d) > 0.9 * StartTowards.Length)
                    throw new FormatException("The startTowards vector must not be parallel to the direction vector");
                StartTowards -= d * Direction;
            }

            StartTowards = Vector3D.Normalize(StartTowards);
        }

        public override List<Vector3D<double>> GetCurve()
        {
            NormalizeVectors();
            double maxT = Length * MathUtils.TwoPi / Step;
            double halfLength = Length / 2;

            List<Vector3D<double>> curve = new();
            for (uint i = 0; i <= Segments; i++)
            {
                double t = i / (double)Segments;
                Matrix4X4<double> mat = Matrix4X4.CreateFromAxisAngle(Direction, maxT * t);

                curve.Add(
                    Center
                    + Direction * MathUtils.Lerp(-halfLength, halfLength, t)
                    + Radius * Vector3D.Transform(StartTowards, mat)
                );
            }

            return curve;
        }
    }
}
