using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TPLinkSmartDevices.Data
{
    public class LightDetails
    {
        [JsonProperty("lamp_beam_angle")]
        public int LampBeamAngle { get; set; }

        [JsonProperty("min_voltage")]
        public int MinVoltage { get; set; }

        [JsonProperty("max_voltage")]
        public int MaxVoltage { get; set; }

        [JsonProperty("max_lumens")]
        public int MaxLumens { get; set; }

        [JsonProperty("wattage")]
        public int Wattage { get; set; }

        [JsonProperty("incandescent_equivalent")]
        public int IncandescentEquivalent { get; set; }

        [JsonProperty("color_rendering_index")]
        public int ColorRenderingIndex { get; set; }
    }
}
