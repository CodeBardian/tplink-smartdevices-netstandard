# Getting Started

## Installation

Use NuGet package manager to add a reference to this project

=== ".NET CLI"
    ``` 
    > dotnet add package tplink-smartdevices --version 2.0.0
    ```
=== "PackageReference"
    ``` html
    <PackageReference Include="tplink-smartdevices" Version="2.0.0" />
    ```
=== "Package Manager"
    ```
    Install-Package tplink-smartdevices -Version 2.0.0
    ```


!!! note ".NET Standard"
    Be aware that this library targets .net standard, which can not be implemented from all of .net platform versions. To see whether the platform you intend to run on is supported take a look [here](https://github.com/dotnet/standard/blob/master/docs/versions.md).

## Supported Devices

| Class                   | Supported Devices | Not tested, maybe working         |
| ----------------------- | ----------------- |---------------------------------- |
| `TPLinkSmartPlug`       |  HS100            | HS105                             |
| `TPLinkSmartMeterPlug`  |  HS110            |                                   |
| `TPLinkSmartBulb`       | KL100/KL110/KL130 | KL50/KL60/LB100/LB110/LB120/LB130 |
| `TPLinkSmartMultiPlug`  |  HS300/HS107      | KP200/KP303/KP400                 |
| `TPLinkSmartDimmer`     |  HS220            |                                   |

## Usage

### First Use

If your devices are already connected to your Wi-Fi network (e.g through TP-Link provided mobile app Kasa) this step can be skipped. Otherwise you can use the following script to associate your smart devices with your home network:

``` csharp
await new TPLinkDiscovery().Associate("ssid", "password");
```

<small> Full reference for [`TPLinkDiscovery().Associate(string, string)`](docs/discovery.md#associatestring-string-int)</small>

!!! caution
    The device running the above script needs to be connected to the network which the tplink smart devices provide. They should be called "TP-Link_Smart Plug_XXXX" or similar. If you have a brand new plug/bulb this network should automatically appear. Otherwise, hold down the reset button on a plug for about 10 seconds, until its light blinks amber rapidly. For a bulb flip the switch on and off 5 times. Not too quickly though! (About 1 sec per flip).

### Discovery

Smart devices which are already connected to the same network as the host devices (PC, tablet, phone, ...) can be discovered to establish further communcation such as turning the device on/off. The discovery runs in an async `Task<List<TPLinkSmartDevice>>`. There is the possibility to register an event handler which triggers on each discovered device. If the ip-address of smart devices are known and not changing, an object of their associated [classes](#supported-devices) can be created manually without the need for discovery.

=== "Basic"
    ``` csharp
    var discoveredDevices = await new TPLinkDiscovery().Discover();
    ```
    <small> Full reference for [`TPLinkDiscovery.Discover()`](docs/discovery.md#discover)</small>
=== "With event"
    ``` csharp
    TPLinkDiscovery discovery = new TPLinkDiscovery();
    discovery.DeviceFound += delegate {
        ...
        Console.WriteLine($"Device found: {e.Device.Alias}");
        ...
    };
    var discoveredDevices = await discovery.Discover();
    ```
    <small> Full reference for [`TPLinkDiscovery.DeviceFound`](docs/discovery.md#devicefound)</small>
=== "Manual instantiation"
    ``` csharp
    //with constructor (blocking!)
    var smartPlug = new TPLinkSmartPlug("100.10.4.1");
    //or with async factory method
    var smartBulb = await TPLinkSmartPlug.Create("100.10.4.1");
    ```
    <small> Full reference for [`TPLinkSmartPlug`](docs/devices/plug.md) and [`TPLinkSmartBulb`](docs/devices/bulb.md)</small>

### Basic Usage Examples

Following script is a basic example which describes the use-case of turning on all smart plugs in your current network:

``` csharp
var discoveredDevices = await new TPLinkDiscovery().Discover();

foreach (var item in discoveredDevices)
{
    if (item is TPLinkSmartPlug plug)
    {
        await plug.SetPoweredOn(true);
    }
}
```
<small> Full reference for [`TPLinkSmartPlug.SetPoweredOn(bool)`](docs/devices/plug.md#power)</small>

Changing color of a single smart bulb (LB130, KL130):

``` csharp
var smartBulb = await TPLinkSmartBulb.Create("100.10.4.1");

BulbHSV red = new BulbHSV { Hue = 0, Saturation = 100, Value = 100 }; // red HSV(0, 100, 100)
BulbHSV yellow = new BulbHSV { Hue = 60, Saturation = 100, Value = 100 };  // yellow HSV(60, 100, 100)

//apply color (instant)
smartBulb.SetHSV(red);
//apply color with transition time
smartBulb.SetHSV(yellow, 1000);
```
<small> Full reference for [`TPLinkSmartBulb.SetHSV(BulbHSV, int)`](docs/devices/bulb.md#sethsv)</small>

### Remote Control 

If you want to control your devices remotely (not from within the same network) there is the possibility to link each device independently to your kasa account. It then shows up in your Kasa app and can be controlled over the internet from wherever it's needed.

``` csharp
smartDevice.ConfigureRemoteAccess("username", "password");
```
<small> Full reference for [`TPLinkSmartDevice.ConfigureRemoteAccess(string, string)`](docs/devices/device.md#configureremoteaccessstring-string)</small>

### Timer

By setting up a Countdown Rule it is possible to have a device execute a specific action after a certain time runs out. This can for example be used to turn off all devices after half an hour:

``` csharp
List<ICountDown> cdDevices = discoveredDevices.OfType<ICountDown>().ToList();
cdDevices.ForEach(d =>
    d.AddCountDownRule(
        new CountDownRule() { 
            Delay = 1800, 
            Enabled = true, 
            PoweredOn = false, 
            Name = "MyTimer" 
        }
    )
);
```
<small> Full reference for [`CountDownRules`](timer.md)</small>

### Schedule
Schedule your smart devices to automatically switch on or off if you are home or away, on sunrise or sunset or whenever you feel like. Example of turning light bulb on each workday at 07:00 in the morning.
``` csharp
Schedule schedule = new Schedule
    {
        Name = "MySchedule",
        StartAction = 1,
        StartTime = new TimeSpan(7, 0, 0),
        StartTimeOption = TimeOption.Custom,
        Enabled = true,
        Weekdays = Weekdays.WorkDays
    };
await smartBulb.AddSchedule(schedule);
```
<small> Full reference for [`Schedule`](schedule.md)</small>