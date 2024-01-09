using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Magneplot.Generator.Curves;
using Magneplot.Generator.Models;

namespace Magneplot.Generator
{
    record Config(ModelSource ModelSource, CurveSource CurveSource, double I = 1)
    {
        public string Name => ModelSource.Name + "-" + CurveSource.Name;

        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = true,
            IncludeFields = true,
            IgnoreReadOnlyProperties = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        public void SerializeToFile(string filename)
        {
            JsonWriterOptions opts = new() { Indented = true };
            using Utf8JsonWriter writer = new(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read), opts);
            JsonObject s = Serialize();
            s.WriteTo(writer);
        }

        public JsonObject Serialize()
        {
            JsonObject obj = new(new JsonNodeOptions() { PropertyNameCaseInsensitive = true })
            {
                { "model", SerializeModel(serializerOptions) },
                { "curve", SerializeCurve(serializerOptions) },
                { "I", I },
            };

            return obj;
        }

        private JsonObject SerializeModel(JsonSerializerOptions? options = null)
        {
            string typeName;
            JsonElement ele;

            if (ModelSource is PlaneSource planeSource)
            {
                typeName = "plane";
                ele = JsonSerializer.SerializeToElement(planeSource, options);
            }
            else if (ModelSource is CilinderSource cilinderSource)
            {
                typeName = "cilinder";
                ele = JsonSerializer.SerializeToElement(cilinderSource, options);
            }
            else if (ModelSource is ObjFileSource objFileSource)
            {
                typeName = "objfile";
                ele = JsonSerializer.SerializeToElement(objFileSource, options);
            }
            else
            {
                throw new NotImplementedException("Json model serialize type not implemented: " + ModelSource.GetType().FullName);
            }

            JsonNodeOptions jsonObjProps = new() { PropertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive ?? false };

            return new JsonObject(jsonObjProps)
            {
                { "type", typeName },
                { "config", JsonObject.Create(ele, jsonObjProps) },
            };
        }

        private JsonObject SerializeCurve(JsonSerializerOptions? options = null)
        {
            string typeName;
            JsonElement ele;

            if (CurveSource is LineSource lineSource)
            {
                typeName = "line";
                ele = JsonSerializer.SerializeToElement(lineSource, options);
            }
            else if (CurveSource is CircumferenceSource circumferenceSource)
            {
                typeName = "circumference";
                ele = JsonSerializer.SerializeToElement(circumferenceSource, options);
            }
            else if (CurveSource is SpiralSource spiralSource)
            {
                typeName = "spiral";
                ele = JsonSerializer.SerializeToElement(spiralSource, options);
            }
            else
            {
                throw new NotImplementedException("Json curve serialize type not implemented: " + CurveSource.GetType().FullName);
            }

            JsonNodeOptions jsonObjProps = new() { PropertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive ?? false };

            return new JsonObject(jsonObjProps)
            {
                { "type", typeName },
                { "config", JsonObject.Create(ele, jsonObjProps) },
            };
        }

        public static Config DeserializeFromFile(string filename)
        {
            JsonNodeOptions nodeOpts = new() { PropertyNameCaseInsensitive = serializerOptions.PropertyNameCaseInsensitive };
            JsonDocumentOptions docOpts = new() { AllowTrailingCommas = serializerOptions.AllowTrailingCommas, CommentHandling = serializerOptions.ReadCommentHandling };

            using FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Deserialize(JsonNode.Parse(stream, nodeOpts, docOpts).AsObject());
        }

        public static Config Deserialize(JsonObject obj)
        {
            return new Config(
                DeserializeModel(obj["model"]?.AsObject() ?? throw new JsonException("\"model\" field not found on JSON object"), serializerOptions),
                DeserializeCurve(obj["curve"]?.AsObject() ?? throw new JsonException("\"curve\" field not found on JSON object"), serializerOptions),
                obj["I"].GetValue<double>()
            );
        }

        private static ModelSource DeserializeModel(JsonObject obj, JsonSerializerOptions? options = null)
        {
            string type = obj["type"]?.GetValue<string>() ?? throw new JsonException("\"type\" field not found in JSON model object");
            JsonObject conf = obj["config"]?.AsObject() ?? throw new JsonException("\"config\" field not found in JSON model object");

            if (type.Equals("plane", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<PlaneSource>(conf, options);
            }
            else if (type.Equals("cilinder", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<CilinderSource>(conf, options);
            }
            else if (type.Equals("objfile", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<ObjFileSource>(conf, options);
            }
            else
            {
                throw new NotImplementedException("Unknown JSON model type to deserialize: " + type);
            }
        }

        private static CurveSource DeserializeCurve(JsonObject obj, JsonSerializerOptions? options = null)
        {
            string type = obj["type"]?.GetValue<string>() ?? throw new JsonException("\"type\" field not found in JSON curve object");
            JsonObject conf = obj["config"]?.AsObject() ?? throw new JsonException("\"config\" field not found in JSON curve object");

            if (type.Equals("line", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<LineSource>(conf, options);
            }
            else if (type.Equals("circumference", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<CircumferenceSource>(conf, options);
            }
            else if(type.Equals("spiral", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Deserialize<SpiralSource>(conf, options);
            }
            else
            {
                throw new NotImplementedException("Unknown JSON curve type to deserialize: " + type);
            }
        }
    }
}
