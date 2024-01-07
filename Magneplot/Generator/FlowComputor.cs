using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Magneplot.Generator
{
    static class FlowComputor
    {
        const double Mo = 4 * Math.PI * 0.0000001;
        const double MoOver4Pi = 0.0000001;
        const double I = 1;
        public static void ComputeFlowValues(List<Face> model, List<Vector3D<double>> curve)
        {
            int currentCount = 0;
            Parallel.For(0, model.Count, faceIndex =>
            {
                Face face = model[faceIndex];

                Vector3D<double> center = (face.Vertex1 + face.Vertex2 + face.Vertex3) / 3;

                Vector3D<double>[] magfieldVecs = new Vector3D<double>[curve.Count - 1];

                for (int i = 0; i < curve.Count - 1; i++)
                {
                    Vector3D<double> rp = curve[i];
                    Vector3D<double> rp_next = curve[i + 1];

                    Vector3D<double> ds = rp_next - rp;
                    magfieldVecs[i] = MoOver4Pi * I * Vector3D.Cross(ds, center - rp) / Math.Pow((center - rp).LengthSquared, 1.5);
                }

                Vector3D<double> netMagfield = MathUtils.AddAll(magfieldVecs);
                Vector3D<double> areaVec = face.Area * Vector3D.Normalize(face.Normal);
                face.Flow = Vector3D.Dot(netMagfield, areaVec);

                model[faceIndex] = face;

                int num = Interlocked.Increment(ref currentCount);
                if (num % 1000 == 0)
                {
                    Console.WriteLine("Processed {0} faces out of {1} ({2}%)", num, model.Count, Math.Round(100.0 * num / model.Count, 2));
                }
            });
        }
    }
}
