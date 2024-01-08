using System.Collections.Generic;
using System.IO;
using System.Linq;
using Silk.NET.Maths;

namespace Magneplot.Generator
{
    static class CurveFile
    {
        public static void SaveToStream(StreamWriter stream, List<Vector3D<double>> curve)
        {
            stream.Write("{0}\n", curve.Count);

            foreach (var point in curve)
            {
                stream.Write("{0:R} {1:R} {2:R}\n", point.X, point.Y, point.Z);
            }
        }

        public static Vector3D<double>[] FromStream(StreamReader stream)
        {
            uint length = uint.Parse(stream.ReadLine());

            Vector3D<double>[] curve = new Vector3D<double>[length];
            for (int i = 0; i < length; i++)
            {
                string line = stream.ReadLine();
                double[] en = line.Split(' ').Select(double.Parse).ToArray();
                curve[i] = new Vector3D<double>(en[0], en[1], en[2]);
            }

            return curve;
        }
    }
}
