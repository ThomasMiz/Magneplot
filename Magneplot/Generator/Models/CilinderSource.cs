using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Magneplot.Generator.Models
{
    class CilinderSource : ModelSource
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
        public uint RotationalSlices { get; set; }

        [JsonRequired]
        public uint VerticalSlices { get; set; }

        public override string Name
        {
            get
            {
                NormalizeVectors();
                uint hashId = MathUtils.HashToUint(
                    Center.X, Center.Y, Center.Z, Direction.X, Direction.Y, Direction.Z,
                    StartTowards.X, StartTowards.Y, StartTowards.Z, Radius, Length,
                    RotationalSlices, VerticalSlices
                );

                return "Cilinder." + hashId;
            }
        }

        public void NormalizeVectors()
        {
            Direction = Vector3D.Normalize(Direction);

            if (StartTowards == default)
            {
                StartTowards = MathUtils.FindNormalVectorTo(Direction);
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

        public override List<Face> GetModel()
        {
            NormalizeVectors();
            double halfLength = Length / 2;

            List<Face> cilinder = [];

            for (uint ri = 0; ri < RotationalSlices; ri++)
            {
                double r1 = ri / (double)RotationalSlices;
                double r2 = (ri + 1) / (double)RotationalSlices;

                Matrix4X4<double> mat1 = Matrix4X4.CreateFromAxisAngle(Direction, r1 * MathUtils.TwoPi);
                Matrix4X4<double> mat2 = Matrix4X4.CreateFromAxisAngle(Direction, r2 * MathUtils.TwoPi);

                for (uint yi = 0; yi < VerticalSlices; yi++)
                {
                    double y1 = yi / (double)VerticalSlices;
                    double y2 = (yi + 1) / (double)VerticalSlices;

                    Vector3D<double> v1 = Center
                        + Direction * MathUtils.Lerp(-halfLength, halfLength, y1)
                        + Radius * Vector3D.Transform(StartTowards, mat1);

                    Vector3D<double> v2 = Center
                        + Direction * MathUtils.Lerp(-halfLength, halfLength, y2)
                        + Radius * Vector3D.Transform(StartTowards, mat1);

                    Vector3D<double> v3 = Center
                        + Direction * MathUtils.Lerp(-halfLength, halfLength, y2)
                        + Radius * Vector3D.Transform(StartTowards, mat2);

                    Vector3D<double> v4 = Center
                        + Direction * MathUtils.Lerp(-halfLength, halfLength, y1)
                        + Radius * Vector3D.Transform(StartTowards, mat2);

                    cilinder.Add(new Face(v2, v1, v4, Vector3D.Cross(v2 - v1, v2 - v4), 0));
                    cilinder.Add(new Face(v2, v4, v3, Vector3D.Cross(v2 - v4, v2 - v3), 0));
                }
            }

            return cilinder;
        }
    }
}
