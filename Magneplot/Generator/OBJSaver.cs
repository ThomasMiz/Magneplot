using System.Collections.Generic;
using System.IO;
using Silk.NET.Maths;

namespace Magneplot.Generator
{
    internal static class OBJSaver
    {
        private static uint getValueOrInsertAsCount<T>(Dictionary<T, uint> dict, List<T> list, T value)
        {
            if (dict.TryGetValue(value, out var thingy))
                return thingy;

            uint thingy2 = (uint)list.Count + 1;
            dict.Add(value, thingy2);
            list.Add(value);
            return thingy2;
        }
        public static void SaveToFile(List<Face> model, string fileName)
        {
            using StreamWriter stream = new StreamWriter(File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read));
            SaveToStream(model, stream);
        }

        public static void SaveToStream(List<Face> model, StreamWriter stream)
        {
            List<Vector3D<double>> vertex = new();
            List<Vector3D<double>> normals = new();
            List<Vector2D<double>> texCoords = new();

            Dictionary<Vector3D<double>, uint> vertexIds = new();
            Dictionary<Vector3D<double>, uint> normalIds = new();
            Dictionary<Vector2D<double>, uint> texCoordIds = new();

            // (positionId, texCoordId, normalId)
            List<((uint, uint, uint), (uint, uint, uint), (uint, uint, uint))> faces = new();

            foreach (Face face in model)
            {
                uint vertex1Id = getValueOrInsertAsCount(vertexIds, vertex, face.Vertex1);
                uint vertex2Id = getValueOrInsertAsCount(vertexIds, vertex, face.Vertex2);
                uint vertex3Id = getValueOrInsertAsCount(vertexIds, vertex, face.Vertex3);
                uint normalId = getValueOrInsertAsCount(normalIds, normals, face.Normal);
                uint texCoordId = getValueOrInsertAsCount(texCoordIds, texCoords, new Vector2D<double>(face.FlowIntensity, 0));

                faces.Add(((vertex1Id, texCoordId, normalId), (vertex2Id, texCoordId, normalId), (vertex3Id, texCoordId, normalId)));
            }

            foreach (Vector3D<double> v in vertex)
                stream.Write("v {0:R} {1:R} {2:R}\n", v.X, v.Y, v.Z);

            foreach (Vector3D<double> v in normals)
                stream.Write("vn {0:R} {1:R} {2:R}\n", v.X, v.Y, v.Z);

            foreach (Vector2D<double> v in texCoords)
                stream.Write("vt {0:R} {1:R}\n", v.X, v.Y);

            foreach (var face in faces)
            {
                stream.Write(
                    "f {0}/{1}/{2} {3}/{4}/{5} {6}/{7}/{8}\n",
                    face.Item1.Item1, face.Item1.Item2, face.Item1.Item3,
                    face.Item2.Item1, face.Item2.Item2, face.Item2.Item3,
                    face.Item3.Item1, face.Item3.Item2, face.Item3.Item3
                );
            }
            stream.Close();
        }
    }
}
