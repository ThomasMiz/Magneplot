using Magneplot.Generator.Curves;
using Magneplot.Generator.Models;

namespace Magneplot.Generator
{
    public record Config(ModelSource ModelSource, CurveSource CurveSource)
    {
        public string Name => ModelSource.Name + "-" + CurveSource.Name;

        public static Config PlaneConfig { get; } = new Config(new PlaneSource(1.2, 20, 20, 200), new SpiralSource(0.8, 0, 1.2, 0, 20, 1600));
        public static Config CilinderConfig { get; } = new Config(new CilinderSource(0.3, 0, 40, 150, 2400), new SpiralSource(1.2, 0, 3, 0, 40, 3200));
        public static Config CarpinchoConfig { get; } = new Config(new ObjFileSource("data/carpincho.obj", false), new SpiralSource(5, 0, 0.2, -1, 3, 12000));
    }
}