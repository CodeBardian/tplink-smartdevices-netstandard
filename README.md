# tplink-smartdevices-netstandard
.NET Standard 2.0 Library for Discovering and Operating TP-Link Smart Devices <br><br>
[![Nuget](https://img.shields.io/nuget/v/tplink-smartdevices?style=for-the-badge)](https://www.nuget.org/packages/tplink-smartdevices/)
![Travis (.org)](https://img.shields.io/travis/CodeBardian/tplink-smartdevices-netstandard?style=for-the-badge)

This library allows a developer to discover and operate TP-Link Smart Devices from multiple .NET implementations such as .NET Core, Xamarin, .NET Framework and more. 

This project is migrated to .net standard from Anthony Turner's TP-Link Smart Devices SDK: <br>
https://github.com/anthturner/TPLinkSmartDevices <br>
notable changes include: asynchronous operations, more supported devices, better discovery handling, setup functionality (no need for kasa account and app anymore, except for remote control)

Consult https://github.com/dotnet/standard/blob/master/docs/versions.md to see which .net platform versions can implement this library!

#### Supported Devices

| Type                    | Supported models             | Not tested, maybe working         |
| ----------------------- | ---------------------------- |---------------------------------- |
| Plug                    |  HS100, HS110, HS300, HS107  | HS105, HS200, KP200/KP303/KP400   |
| Bulb                    |  KL100/KL110/KL130           | KL50/KL60/LB100/LB110/LB120/LB130 |
| Switch                  |  HS220                       |                                   |

> Doesn't include new protocol for firmware version 1.1.0 on HS100 (Hardware Version 4.1)

## Usage
Use NuGet package manager to add a reference to this project, for example with dotnet cli:
```
> dotnet add package tplink-smartdevices --version 2.0.1
```

#### Setup / First Use

If your devices are already connected to your Wi-Fi network (e.g through TP-Link provided mobile app Kasa) this step can be skipped. Otherwise you can use the following script to associate your smart devices with your home network:

```cs
await new TPLinkDiscovery().Associate("ssid", "password");
```
Note that the device running the program needs to be connected to the network which the tplink devices provide. It should be called "TP-Link_Smart Plug_XXXX" or similar. If you have a brand new plug/bulb this network should automatically appear. Otherwise, hold down the reset button on a plug for about 10 seconds, until its light blinks amber rapidly. For a bulb flip the switch on and off 5 times. Not too quickly though! (About 1 sec per flip).

### Discovery

basic:
```cs
// Runs in a async Task<List<TPLinkSmartDevice>>
var discoveredDevices = await new TPLinkDiscovery().Discover();
```
	
with event handler:
```cs
TPLinkDiscovery discovery = new TPLinkDiscovery();
discovery.DeviceFound += delegate {
	//Console.WriteLine("Device found: " + e.Device.Alias);
	//Log.Debug("DISCOVERY","Device found" + e.Device.Alias);	
};
var discoveredDevices = await discovery.Discover();
```    

### Power State
```cs
var smartPlug = await TPLinkSmartPlug.Create("100.10.4.1");
await smartPlug.SetPoweredOn(true); // Turn on relay
await smartPlug.SetPoweredOn(false); // Turn off relay
```  
 
or after discovery:
```cs    
foreach (var item in discoveredDevices)
{
    if (item is TPLinkSmartPlug plug)
    {
        await plug.SetPoweredOn(true);
    }
    else if (item is TPLinkSmartBulb bulb) 
    {
        await bulb.SetPoweredOn(true);
    }
}
```  

### Timer
```cs
CountDownRule cdr = new CountDownRule() { 
    Delay = 3600, 
    Enabled = true, 
    PoweredOn = true, 
    Name = "1h Timer" 
}
await plug.AddCountDownRule(cdr);
```

### Schedule
```cs
Schedule schedule = new Schedule
{
    Name = "TurnOffMondays10am",
    StartAction = 0,
    StartTime = new TimeSpan(10, 0, 0),
    StartTimeOption = TimeOption.Custom,
    Enabled = true,
    Weekdays = Weekdays.Monday,
};
await plug.AddSchedule(schedule);
```
    
### Remote Control

If you still want to control your devices remotely (not from within the same network) there is the possibility to link each device independently to your kasa account. It then shows up in your app.
```cs
smartdevice.ConfigureRemoteAccess("username", "password");
```
To check if your device is linked to tplink cloud use `RemoteAccessEnabled` property.

## Disclaimer
I can not guarantee the functionality of this library as I only tested a HS100 and a KL130 in a Xamarin.Android application yet.

This library has no affiliation with TP-Link.
TP-Link and all respective product names are copyright TP-Link Technologies Co, Ltd. and/or its subsidiaries and affiliates.
