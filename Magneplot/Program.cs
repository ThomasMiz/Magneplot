using System;
using System.Numerics;
using System.IO;
using Magneplot.Window;
using Magneplot.Generator;
using Magneplot.Generator.Curves;
using Magneplot.Generator.Models;
using Silk.NET.Maths;
using System.Collections.Generic;
using System.Linq;
using TrippyGL;

namespace Magneplot
{
    internal class Program
    {
        static string OutputFileBasePath = "models";

        static void Main(string[] args)
        {
            Config config = Config.PlaneConfig;
            //Config config = Config.CilinderConfig;
            //Config config = Config.CarpinchoConfig;

            string filename = Path.Combine(OutputFileBasePath, config.Name + ".mobj");

            VertexPosition[] curveVertex;
            VertexNormalTexture[] modelVertex;
            double netFlow;

            if (File.Exists(filename))
            {
                Console.WriteLine("Reading file: {0}", filename);
                using StreamReader stream = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
                netFlow = double.Parse(stream.ReadLine());
                curveVertex = CurveFile.FromStream(stream).Select(v => new VertexPosition((float)v.X, (float)v.Y, (float)v.Z)).ToArray();
                modelVertex = OBJLoader.FromStream<VertexNormalTexture>(stream);
            }
            else
            {
                Console.WriteLine("Couldn't file file {0}, generating...", filename);
                Console.WriteLine("Building mobj file for config {0}", config.Name);

                ModelSource modelSource = config.ModelSource;
                List<Face> model = modelSource.GetModel();
                Console.WriteLine("Got model with {0} faces", model.Count);

                CurveSource curveSource = config.CurveSource;
                List<Vector3D<double>> curve = curveSource.GetCurve();
                Console.WriteLine("Got curve with {0} segments", curve.Count - 1);

                FlowComputor.ComputeFlowValues(model, curve, config.I);
                netFlow = MathUtils.AddAll(model.Select(v => v.Flow).ToArray());

                Console.WriteLine("Saving to file: {0}", filename);
                Directory.CreateDirectory(OutputFileBasePath);
                using StreamWriter stream = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read));
                stream.Write("{0:R}\n", netFlow);
                CurveFile.SaveToStream(stream, curve);
                OBJSaver.SaveToStream(model, stream);

                curveVertex = curve.Select(v => new VertexPosition((float)v.X, (float)v.Y, (float)v.Z)).ToArray();

                modelVertex = new VertexNormalTexture[model.Count * 3];
                for (int i = 0; i < model.Count; i++)
                {
                    Face face = model[i];

                    modelVertex[i * 3] = new VertexNormalTexture(
                        face.Vertex1.As<float>().ToSystem(),
                        face.Normal.As<float>().ToSystem(),
                        new Vector2((float)face.FlowIntensity, 0)
                    );

                    modelVertex[i * 3 + 1] = new VertexNormalTexture(
                        face.Vertex2.As<float>().ToSystem(),
                        face.Normal.As<float>().ToSystem(),
                        new Vector2((float)face.FlowIntensity, 0)
                    );

                    modelVertex[i * 3 + 2] = new VertexNormalTexture(
                        face.Vertex3.As<float>().ToSystem(),
                        face.Normal.As<float>().ToSystem(),
                        new Vector2((float)face.FlowIntensity, 0)
                    );
                }
            }

            new MagneplotWindow(netFlow, modelVertex, curveVertex).Run();
        }
    }
}