namespace TPLinkSmartDevices.Devices
{
    internal class DimmerOptions
    {
        public DimmerOptions()
        {
            FadeOnTime = 2000;
            FadeOffTime = 2000;
            GentleOnTime = 2000;
            GentleOffTime = 2000;
            Mode = DimmerMode.GentleOnOff;
        }

        public int FadeOnTime { get; set; }
        public int FadeOffTime { get; set; }
        public int GentleOnTime { get; set; }
        public int GentleOffTime { get; set; }

        public DimmerMode Mode { get; set; }
    }

    public enum DimmerMode
    {
        GentleOnOff,
        InstantOnOff,
        Preset,
        None
    }
}