
All devices implementing `ISchedule` can perform certain actions repeatedly on a specific time. Use `Schedule` to specify the options. Schedules are executed when the time of day equals `StartTime` as long as
the schedule is `Enabled`. All created schedules can be accessed via the `Schedules` property.

``` csharp
Schedule schedule = new Schedule
{
    Name = "Test1",
    StartAction = 1,
    StartTime = new TimeSpan(13, 1, 0),
    StartTimeOption = TimeOption.Custom,
    Enabled = true,
    Weekdays = Weekdays.WeekendDays,
};
await device.AddSchedule(schedule);
```

#### Options

| Property                | Type              | Description                       |
| ----------------------- | ----------------- |---------------------------------- |
| `Name`                  |  string           | custom name of schedule, shows in kasa app |
| `Enabled`               |  bool             | if the schedule is currently active or not  |
| `StartAction`           |  int              | whether to turn device on or off at start of rule. 0 = turn off, 1 = turn on |
| `StartTimeOption`       |  TimeOption       | `TimeOption.Sunset` and  `TimeOption.Sunrise` trigger the action at, well, sunset or sunrise. Use  `TimeOption.Custom` in conjunction with `StartTime` to set your own timing|
| `StartTime`             |  TimeSpan         | time on which the action triggers when using `StartTimeOption = TimeOption.Custom` e.g. 13:05 would be `TimeSpan(13, 5, 0)` or you can specify in minutes after midnight with `TimeSpan.FromMinutes(785)` |
| `Weekdays`              |  Weekdays         | flag of days on which the schedule is executed. Combine multiple days with bitwise or - operator `Weekdays = Weekdays.Monday | Weekdays.Friday` or use preset combinations like `Weekdays = Weekdays.WeekendDays`|

<small>some more options are available, they are not well tested as of version 2.0.0</small>

#### Methods

``` csharp
Task ICountDown.RetrieveSchedules();
```
queries the device for current schedules and updates `Schedules` respectively.

---
``` csharp
Task ICountDown.AddSchedule(Schedule);
```
adds a new schedule

---
``` csharp
Task ICountDown.EditSchedule(Schedule);
```
update an existing schedule. Example:
``` csharp
device.Schedule[0].Weekdays |= Weekdays.Thursday;
await device.EditSchedule(Schedule[0]);
```

---
``` csharp
Task ICountDown.DeleteSchedule(Schedule);
```
deletes an existing schedule. Example:
``` csharp
await device.DeleteSchedule(Schedules[0]);
```

---
``` csharp
Task ICountDown.Schedules();
```
deletes all existing schedules.