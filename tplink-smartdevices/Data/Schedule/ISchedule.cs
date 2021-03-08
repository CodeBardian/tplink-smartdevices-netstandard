using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TPLinkSmartDevices.Data.Schedule
{
    interface ISchedule
    {
        List<Schedule> Schedules { get; }
        Task RetrieveSchedules();
        Task AddSchedule(Schedule schedule);
        Task EditSchedule(Schedule schedule);
        Task DeleteSchedule(string id);
        Task DeleteSchedules();
    }
}
