using System.Collections.Generic;
using Silk.NET.Maths;

namespace Magneplot.Generator.Curves
{
    abstract class CurveSource
    {
        public abstract string Name { get; }

        public abstract List<Vector3D<double>> GetCurve();
    }
}
