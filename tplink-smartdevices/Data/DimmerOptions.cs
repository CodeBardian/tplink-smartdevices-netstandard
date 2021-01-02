using System;

namespace TPLinkSmartDevices.Devices
{
    public class DimmerOptions
    {
        public DimmerOptions()
        {
            FadeOnTime = 2000;
            FadeOffTime = 2000;
            GentleOnTime = 2000;
            GentleOffTime = 2000;
            DoubleClickAction = DimmerMode.GentleOnOff;
            LongPressAction = DimmerMode.GentleOnOff;
            Mode = DimmerMode.GentleOnOff;
        }

        public int FadeOnTime { get; set; }
        public int FadeOffTime { get; set; }
        public int GentleOnTime { get; set; }
        public int GentleOffTime { get; set; }

        public DimmerMode DoubleClickAction { get; set; }
        public DimmerMode LongPressAction { get; set; }
        public DimmerMode Mode { get; set; }
    }

    public enum DimmerMode
    {
        GentleOnOff,
        InstantOnOff,
        Preset,
        None
    }

    public static class DimmerModeExtensions
    {
        public static string ToStr(this DimmerMode mode)
        {
            return mode switch
            {
                DimmerMode.None => "none",
                DimmerMode.GentleOnOff => "gentle_on_off",
                DimmerMode.InstantOnOff => "instant_on_off",
                DimmerMode.Preset => "customize_preset",
                _ => "",
            };
        }

        public static DimmerMode ToDimmerMode(this string str)
        {
            return str switch
            {
                "none" => DimmerMode.None,
                "gentle_on_off" => DimmerMode.GentleOnOff,
                "instant_on_off" => DimmerMode.InstantOnOff,
                "customize_preset" => DimmerMode.Preset,
                _ => throw new ArgumentException($"can't parse {str} to DimmerMode"),
            };
        }
    }
}