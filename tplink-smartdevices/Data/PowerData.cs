using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Data
{
    /// <summary>
    /// Encapsulates JSON data structure for current energy use as metered by the HS110 Smart Energy Meter.
    /// </summary>
    public class PowerData
    {
        private readonly dynamic _powerData;
        private string _hwversion;

        public PowerData(dynamic powerData, string hwversion)
        {
            _powerData = powerData;
            _hwversion = hwversion;
        }

        /// <summary>
        /// Currently measured voltage in volts.
        /// </summary>
        public double Voltage => _hwversion.StartsWith("1") ? _powerData.voltage : _powerData.voltage_mv / 1000.0d;

        /// <summary>
        /// Currently measured current in amperes.
        /// </summary>
        public double Amperage => _hwversion.StartsWith("1") ? _powerData.current : _powerData.current_ma / 1000.0d;

        /// <summary>
        /// Currently measured power in watts.
        /// </summary>
        public double Power => _hwversion.StartsWith("1") ? _powerData.power : _powerData.power_mw / 1000.0d;

        /// <summary>
        /// Total power consumption in kilowatthours. 
        /// </summary>
        public double Total => _hwversion.StartsWith("1") ? _powerData.total : _powerData.total_wh / 1000.0d;

        public int ErrorCode => _powerData.err_code;

        public override string ToString()
        {
            return $"{Voltage:0.00} Volt, {Amperage:0.00} Ampere, {Power:0.00} Watt";
        }
    }
}
