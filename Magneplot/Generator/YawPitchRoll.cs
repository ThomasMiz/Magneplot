namespace Magneplot.Generator
{
    record struct YawPitchRoll(double Yaw, double Pitch, double Roll)
    {
        public static YawPitchRoll Zero { get; } = default;
    }
}
