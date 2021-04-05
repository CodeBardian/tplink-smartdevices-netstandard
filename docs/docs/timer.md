
All Devices implementing `ICountDown` can perform certain actions after a timer runs out. Use `CountDownRule` to specify the timer options. All timers only run once, for repeated actions create a [`Schedule`](timer.md). 
Currently active timers can be accessed via the `CountDownRules` property.

``` csharp
var cdr = new CountDownRule() { 
    Delay = 1800, 
    Enabled = true, 
    PoweredOn = false, 
    Name = "MyTimer" 
}
await device.AddCountDownRule(cdr);
```

#### Options

| Property                | Type              | Description                       |
| ----------------------- | ----------------- |---------------------------------- |
| `Name`                  |  string           | custom name of timer, shows in kasa app |
| `Enabled`               |  bool             | if the rule is currently active or not  |
| `PoweredOn`             |  bool             | if the device should be powered on or off after the timer runs out |
| `Delay`                 |  int              | delay in seconds after which the action triggers                   |

#### Methods

``` csharp
Task ICountDown.RetrieveCountDownRules();
```
queries the device for current timers and updates `CountDownRules` respectively.

---
``` csharp
Task ICountDown.AddCountDownRule(CountDownRule);
```
adds a new rule

---
``` csharp
Task ICountDown.EditCountDownRule(CountDownRule);
```
update an existing rule. Example:
``` csharp
device.CountDownRules[0].Delay = 36000;
await device.EditCountDownRule(CountDownRules[0]);
```

---
``` csharp
Task ICountDown.DeleteCountDownRule(CountDownRule);
```
deletes an existing rule. Example:
``` csharp
await device.DeleteCountDownRule(CountDownRules[0]);
```

---
``` csharp
Task ICountDown.DeleteAllCountDownRules();
```
deletes all existing rules.