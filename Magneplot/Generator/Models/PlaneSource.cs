using System.Collections.Generic;
using Silk.NET.Maths;

namespace Magneplot.Generator.Models
{
    public class PlaneSource : ModelSource
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public uint HorizontalSlices { get; set; }
        public uint VerticalSlices { get; set; }

        public override string Name
        {
            get
            {
                uint hashId = MathUtils.HashToUint(MinX, MaxX, MinY, MaxY, HorizontalSlices, VerticalSlices);
                return "Plane." + hashId;
            }
        }

        public PlaneSource(double width, double height, uint horizontalSlices, uint verticalSlices)
        {
            MaxX = width / 2.0;
            MinX = -MaxX;
            MinY = 0;
            MaxY = height;
            HorizontalSlices = horizontalSlices;
            VerticalSlices = verticalSlices;
        }

        public override List<Face> GetModel()
        {
            Vector3D<double> normal = Vector3D<double>.UnitZ;

            List<Face> plane = new();
            for (uint yi = 0; yi < VerticalSlices; yi++)
            {
                for (uint xi = 0; xi < HorizontalSlices; xi++)
                {
                    double fromX = MathUtils.Lerp(MinX, MaxX, xi / (double)HorizontalSlices);
                    double toX = MathUtils.Lerp(MinX, MaxX, (xi + 1) / (double)HorizontalSlices);
                    double fromY = MathUtils.Lerp(MinY, MaxY, yi / (double)VerticalSlices);
                    double toY = MathUtils.Lerp(MinY, MaxY, (yi + 1) / (double)VerticalSlices);

                    plane.Add(new Face(
                        new Vector3D<double>(fromX, fromY, 0),
                        new Vector3D<double>(fromX, toY, 0),
                        new Vector3D<double>(toX, fromY, 0),
                        normal,
                        0
                    ));

                    plane.Add(new Face(
                        new Vector3D<double>(toX, fromY, 0),
                        new Vector3D<double>(fromX, toY, 0),
                        new Vector3D<double>(toX, toY, 0),
                        normal,
                        0
                    ));
                }
            }

            return plane;
        }
    }
}
