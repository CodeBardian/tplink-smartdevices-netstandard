using System;
using System.Collections.Generic;
using System.Text;

namespace TPLinkSmartDevices.Data.CountDownRule
{
    public class CountDownRule
    {
        /// <summary>
        /// identifier of CountDown rule
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// custom name of CountDown rule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// if the rule is currently active or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// if the device should be powered on or off after the timer runs out
        /// </summary>
        public bool PoweredOn { get; set; }

        /// <summary>
        /// delay in seconds after which the action triggers 
        /// </summary>
        public int Delay { get; set; }
    }
}
