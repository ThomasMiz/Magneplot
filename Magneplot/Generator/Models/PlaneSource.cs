using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace Magneplot.Generator.Models
{
    public class PlaneSource : ModelSource
    {
        public Vector3D<double> Center { get; set; } = Vector3D<double>.Zero;

        [JsonRequired]
        public Vector3D<double> HorizontalVector { get; set; }

        [JsonRequired]
        public Vector3D<double> VerticalVector { get; set; }

        [JsonRequired]
        public double Width { get; set; }

        [JsonRequired]
        public double Height { get; set; }

        [JsonRequired]
        public uint HorizontalSlices { get; set; }

        [JsonRequired]
        public uint VerticalSlices { get; set; }

        public override string Name
        {
            get
            {
                NormalizeVectors();
                uint hashId = MathUtils.HashToUint(
                    Center.X, Center.Y, Center.Z,
                    HorizontalVector.X, HorizontalVector.Y, HorizontalVector.Z,
                    VerticalVector.X, VerticalVector.Y, VerticalVector.Z,
                    Width, Height, HorizontalSlices, VerticalSlices
                );

                return "Plane." + hashId;
            }
        }

        public void NormalizeVectors()
        {
            HorizontalVector = Vector3D.Normalize(HorizontalVector);
            VerticalVector = Vector3D.Normalize(VerticalVector);

        }

        public override List<Face> GetModel()
        {
            NormalizeVectors();
            double halfWidth = Width / 2;
            double halfHeight = Height / 2;

            Vector3D<double> normal = Vector3D.Normalize(Vector3D.Cross(HorizontalVector, VerticalVector));

            List<Face> plane = new();
            for (uint yi = 0; yi < VerticalSlices; yi++)
            {
                for (uint xi = 0; xi < HorizontalSlices; xi++)
                {
                    double xmix = xi / (double)HorizontalSlices;
                    double xmix2 = (xi + 1) / (double)HorizontalSlices;
                    double ymix = yi / (double)VerticalSlices;
                    double ymix2 = (yi + 1) / (double)VerticalSlices;

                    Vector3D<double> v1 = Center
                        + HorizontalVector * MathUtils.Lerp(-halfWidth, halfWidth, xmix)
                        + VerticalVector * MathUtils.Lerp(-halfHeight, halfHeight, ymix);

                    Vector3D<double> v2 = Center
                        + HorizontalVector * MathUtils.Lerp(-halfWidth, halfWidth, xmix)
                        + VerticalVector * MathUtils.Lerp(-halfHeight, halfHeight, ymix2);

                    Vector3D<double> v3 = Center
                        + HorizontalVector * MathUtils.Lerp(-halfWidth, halfWidth, xmix2)
                        + VerticalVector * MathUtils.Lerp(-halfHeight, halfHeight, ymix2);

                    Vector3D<double> v4 = Center
                        + HorizontalVector * MathUtils.Lerp(-halfWidth, halfWidth, xmix2)
                        + VerticalVector * MathUtils.Lerp(-halfHeight, halfHeight, ymix);

                    plane.Add(new Face(v1, v2, v4, normal, 0));
                    plane.Add(new Face(v4, v2, v3, normal, 0));
                }
            }

            return plane;
        }
    }
}
