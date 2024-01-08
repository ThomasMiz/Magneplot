using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.NET.Maths;
using TrippyGL;

namespace Magneplot.Generator.Models
{
    class ObjFileSource : ModelSource
    {
        [JsonRequired]
        public string File { get; set; }

        public Vector3D<double> Translation { get; set; } = Vector3D<double>.Zero;
        public Vector3D<double> Scale { get; set; } = Vector3D<double>.One;
        public YawPitchRoll Rotation { get; set; } = YawPitchRoll.Zero;
        public bool OverrideObjNormals { get; set; } = false;

        public override string Name
        {
            get
            {
                uint hashId = MathUtils.HashToUint(
                    File.GetHashCode32(), Translation.X, Translation.Y, Translation.Z,
                    Scale.X, Scale.Y, Scale.Z, Rotation.Yaw, Rotation.Pitch, Rotation.Roll,
                    OverrideObjNormals
                );

                return "Obj." + hashId;
            }
        }

        public override List<Face> GetModel()
        {
            Matrix4X4<double> matrix = Matrix4X4.CreateScale(Scale)
                * Matrix4X4.CreateFromYawPitchRoll(Rotation.Yaw, Rotation.Pitch, Rotation.Roll)
                * Matrix4X4.CreateTranslation(Translation);

            if (OverrideObjNormals)
            {
                return GetModelWithOverridenNormals(matrix);
            }
            else
            {
                return GetModelWithBuiltinNormals(matrix);
            }
        }

        private List<Face> GetModelWithOverridenNormals(in Matrix4X4<double> matrix)
        {
            VertexPosition[] vertices = OBJLoader.FromFile<VertexPosition>(File);
            List<Face> faces = [];

            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3D<double> v1 = new(vertices[i].Position.X, vertices[i].Position.Y, vertices[i].Position.Z);
                Vector3D<double> v2 = new(vertices[i + 1].Position.X, vertices[i + 1].Position.Y, vertices[i + 1].Position.Z);
                Vector3D<double> v3 = new(vertices[i + 2].Position.X, vertices[i + 2].Position.Y, vertices[i + 2].Position.Z);
                v1 = Vector3D.Transform(v1, matrix);
                v2 = Vector3D.Transform(v2, matrix);
                v3 = Vector3D.Transform(v3, matrix);

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

        private List<Face> GetModelWithBuiltinNormals(in Matrix4X4<double> matrix)
        {
            VertexNormal[] vertices = OBJLoader.FromFile<VertexNormal>(File);
            List<Face> faces = [];

            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3D<double> v1 = new(vertices[i].Position.X, vertices[i].Position.Y, vertices[i].Position.Z);
                Vector3D<double> v2 = new(vertices[i + 1].Position.X, vertices[i + 1].Position.Y, vertices[i + 1].Position.Z);
                Vector3D<double> v3 = new(vertices[i + 2].Position.X, vertices[i + 2].Position.Y, vertices[i + 2].Position.Z);
                v1 = Vector3D.Transform(v1, matrix);
                v2 = Vector3D.Transform(v2, matrix);
                v3 = Vector3D.Transform(v3, matrix);

                Vector3D<double> v12 = (v1 + v2) / 2;
                Vector3D<double> v13 = (v1 + v3) / 2;
                Vector3D<double> v23 = (v2 + v3) / 2;

                Vector3D<double> n1 = new(vertices[i].Normal.X, vertices[i].Normal.Y, vertices[i].Normal.Z);
                Vector3D<double> n2 = new(vertices[i + 1].Normal.X, vertices[i + 1].Normal.Y, vertices[i + 1].Normal.Z);
                Vector3D<double> n3 = new(vertices[i + 2].Normal.X, vertices[i + 2].Normal.Y, vertices[i + 2].Normal.Z);
                n1 = Vector3D.TransformNormal(n1, matrix);
                n2 = Vector3D.TransformNormal(n2, matrix);
                n3 = Vector3D.TransformNormal(n3, matrix);

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