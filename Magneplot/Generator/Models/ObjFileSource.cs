using System.Collections.Generic;
using Silk.NET.Maths;
using TrippyGL;

namespace Magneplot.Generator.Models
{
    class ObjFileSource : ModelSource
    {
        public string File { get; }
        public bool OverrideObjNormals { get; }

        public override string Name
        {
            get
            {
                uint hashId = unchecked((uint)File.GetHashCode32());
                return "Obj." + hashId;
            }
        }

        public ObjFileSource(string file, bool overrideObjNormals)
        {
            File = file;
            OverrideObjNormals = overrideObjNormals;
        }

        public override List<Face> GetModel()
        {
            if (OverrideObjNormals)
            {
                return GetModelWithOverridenNormals();
            }
            else
            {
                return GetModelWithBuiltinNormals();
            }
        }

        private List<Face> GetModelWithOverridenNormals()
        {
            VertexPosition[] vertices = OBJLoader.FromFile<VertexPosition>(File);
            List<Face> faces = new();

            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3D<double> v1 = new Vector3D<double>(vertices[i].Position.X, vertices[i].Position.Y, vertices[i].Position.Z);
                Vector3D<double> v2 = new Vector3D<double>(vertices[i + 1].Position.X, vertices[i + 1].Position.Y, vertices[i + 1].Position.Z);
                Vector3D<double> v3 = new Vector3D<double>(vertices[i + 2].Position.X, vertices[i + 2].Position.Y, vertices[i + 2].Position.Z);
                Vector3D<double> v12 = (v1 + v2) / 2;
                Vector3D<double> v13 = (v1 + v3) / 2;
                Vector3D<double> v23 = (v2 + v3) / 2;

                // Subdivide this triangle into 4. Calculate normals based on each triangle's vertices:
                faces.Add(new Face(v1, v12, v13, Vector3D.Cross(v1 - v12, v1 - v13), 0));
                faces.Add(new Face(v12, v2, v23, Vector3D.Cross(v12 - v2, v12 - v23), 0));
                faces.Add(new Face(v13, v23, v3, Vector3D.Cross(v13 - v23, v13 - v3), 0));
                faces.Add(new Face(v12, v23, v13, Vector3D.Cross(v12 - v23, v12 - v13), 0));
            }

            return faces;
        }

        private List<Face> GetModelWithBuiltinNormals()
        {
            VertexNormal[] vertices = OBJLoader.FromFile<VertexNormal>(File);
            List<Face> faces = new();

            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3D<double> v1 = new Vector3D<double>(vertices[i].Position.X, vertices[i].Position.Y, vertices[i].Position.Z);
                Vector3D<double> v2 = new Vector3D<double>(vertices[i + 1].Position.X, vertices[i + 1].Position.Y, vertices[i + 1].Position.Z);
                Vector3D<double> v3 = new Vector3D<double>(vertices[i + 2].Position.X, vertices[i + 2].Position.Y, vertices[i + 2].Position.Z);
                Vector3D<double> v12 = (v1 + v2) / 2;
                Vector3D<double> v13 = (v1 + v3) / 2;
                Vector3D<double> v23 = (v2 + v3) / 2;

                Vector3D<double> n1 = new Vector3D<double>(vertices[i].Normal.X, vertices[i].Normal.Y, vertices[i].Normal.Z);
                Vector3D<double> n2 = new Vector3D<double>(vertices[i + 1].Normal.X, vertices[i + 1].Normal.Y, vertices[i + 1].Normal.Z);
                Vector3D<double> n3 = new Vector3D<double>(vertices[i + 2].Normal.X, vertices[i + 2].Normal.Y, vertices[i + 2].Normal.Z);

                // Subdivide this triangle into 4. Use the model's builtin normals:
                faces.Add(new Face(v1, v12, v13, (4 * n1 + n2 + n3) / 6, 0));
                faces.Add(new Face(v12, v2, v23, (4 * n2 + n1 + n3) / 6, 0));
                faces.Add(new Face(v13, v23, v3, (4 * n3 + n1 + n2) / 6, 0));
                faces.Add(new Face(v12, v23, v13, (n1 + n2 + n3) / 3, 0));
            }

            return faces;
        }
    }
}