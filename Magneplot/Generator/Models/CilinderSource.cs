using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Magneplot.Generator.Models
{
    class CilinderSource : ModelSource
    {
        [JsonRequired]
        public double Radius { get; set; }

        [JsonRequired]
        public double MinY { get; set; }

        [JsonRequired]
        public double MaxY { get; set; }

        [JsonRequired]
        public uint RotationalSlices { get; set; }

        [JsonRequired]
        public uint VerticalSlices { get; set; }

        public override string Name
        {
            get
            {
                uint hashId = MathUtils.HashToUint(Radius, MinY, MaxY, RotationalSlices, VerticalSlices);
                return "Cilinder." + hashId;
            }
        }

        public override List<Face> GetModel()
        {
            List<Face> cilinder = new();

            for (uint ri = 0; ri < RotationalSlices; ri++)
            {
                double r1 = MathUtils.TwoPi * ri / RotationalSlices;
                double r2 = MathUtils.TwoPi * (ri + 1) / RotationalSlices;

                (double sin1, double cos1) = Math.SinCos(r1);
                (double sin2, double cos2) = Math.SinCos(r2);
                double x1 = cos1 * Radius;
                double x2 = cos2 * Radius;
                double z1 = sin1 * Radius;
                double z2 = sin2 * Radius;

                for (uint yi = 0; yi < VerticalSlices; yi++)
                {
                    double y1 = MathUtils.Lerp(MinY, MaxY, yi / (double)VerticalSlices);
                    double y2 = MathUtils.Lerp(MinY, MaxY, (yi + 1) / (double)VerticalSlices);

                    cilinder.Add(new Face(
                        new Vector3D<double>(x1, y1, z1),
                        new Vector3D<double>(x1, y2, z1),
                        new Vector3D<double>(x2, y1, z2),
                        new Vector3D<double>((x1 + x1 + x2) / 3, 0, (z1 + z1 + z2) / 3),
                        0
                    ));

                    cilinder.Add(new Face(
                        new Vector3D<double>(x2, y1, z2),
                        new Vector3D<double>(x1, y2, z1),
                        new Vector3D<double>(x2, y2, z2),
                        new Vector3D<double>((x2 + x1 + x2) / 3, 0, (z2 + z1 + z2) / 3),
                        0
                    ));
                }
            }

            return cilinder;
        }
    }
}
