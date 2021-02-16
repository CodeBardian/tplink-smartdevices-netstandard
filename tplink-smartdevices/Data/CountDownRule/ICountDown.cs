using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TPLinkSmartDevices.Data.CountDownRule;

namespace TPLinkSmartDevices.Devices
{
    public interface ICountDown
    {
        List<CountDownRule> CountDownRules { get; }
        Task RetrieveCountDownRules();
        Task AddCountDownRule(CountDownRule cdr);
        Task EditCountDownRule(string id, bool? enabled = null, int? delay = null, bool? poweredOn = null, string name = null);
        Task EditCountDownRule(CountDownRule cdr);
        Task DeleteCountDownRule(string id);
        Task DeleteAllCountDownRules();
    }
}
