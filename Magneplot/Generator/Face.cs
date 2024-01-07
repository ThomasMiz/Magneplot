using Silk.NET.Maths;

namespace Magneplot.Generator
{
    public record struct Face(Vector3D<double> Vertex1, Vector3D<double> Vertex2, Vector3D<double> Vertex3, Vector3D<double> Normal, double FlowIntensity)
    {
        public double Area => MathUtils.AreaOfTriangle(Vertex1, Vertex2, Vertex3);

        public double Flow => FlowIntensity * Area;
    }
}
