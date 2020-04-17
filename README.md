# tplink-smartdevices-netstandard
.NET Standard 1.6 Library for Discovering and Operating TP-Link Smart Devices <br><br>
[![Nuget](https://img.shields.io/nuget/v/tplink-smartdevices?style=for-the-badge)](https://www.nuget.org/packages/tplink-smartdevices/)

This library allows a developer to discover and operate TP-Link Smart Devices with C# applications such as Xamarin, UWP or .net framework.
This includes support for TP-Link Smart Plugs HS100/105/110 as well as TP-Link Smart Bulbs KL/LB: 100/110/120/130.

This project is migrated to .net standard from Anthony Turner's TP-Link Smart Devices SDK: <br>
https://github.com/anthturner/TPLinkSmartDevices <br>
some changes have been made, e.g added asynchronous code, support of newer KL-series bulbs, event for better discovery handling, setup functionality (no need for kasa account and app anymore, except for remote control)

Consult https://github.com/dotnet/standard/blob/master/docs/versions.md to see which .net platform versions can implement this library before using!
### Prerequisites
~~Before using tplink-smartdevices your devices must be connected to the Wi-Fi network.
This can be done using the TP-Link provided mobile app Kasa.~~

#### Setup / First Use

If your devices are already connected to your Wi-Fi network (e.g through TP-Link provided mobile app Kasa) this step can be skipped. Otherwise you can use the following script to associate your smart devices with your home network:

```
await new TPLinkDiscovery().Associate("ssid", "password");
```
Note that the device running the program needs to be connected to the network which the tplink devices provide. It should be called "TP-Link_Smart Plug_XXXX" or similar. If you have a brand new plug/bulb this network should automatically appear. Otherwise, hold down the reset button on a plug for about 10 seconds, until its light blinks amber rapidly. For a bulb flip the switch on and off 5 times. Not too quickly though! (About 1 sec per flip).

## Usage
Use NuGet package manager to add a reference to this project, for example with dotnet cli:
```
> dotnet add package tplink-smartdevices --version 1.0.2
```

### Discovery

basic:

	// Runs in a async Task<List<TPLinkSmartDevice>>
	var discoveredDevices = await new TPLinkDiscovery().Discover();
	
with event handler:

	TPLinkDiscovery discovery = new TPLinkDiscovery();
	discovery.DeviceFound += delegate {
	    //Console.WriteLine("Device found: " + e.Device.Alias);
	    //Log.Debug("DISCOVERY","Device found" + e.Device.Alias);	
	};
	var discoveredDevices = await discovery.Discover();
	    

### Example Usage
    var smartPlug = new TPLinkSmartPlug("100.10.4.1");
    smartPlug.SetOutletPowered(true); // Turn on relay
    smartPlug.SetOutletPowered(false); // Turn off relay

    var smartBulb = new TPLinkSmartBulb("100.10.4.2");
    smartBulb.SetPoweredOn(true); // Turn on bulb
    smartBulb.SetPoweredOn(false); // Turn off bulb
 
or after discovery:
    
    foreach (var item in discoveredDevices)
    {
        if (item is TPLinkSmartPlug plug)
        {
            plug.SetOutletPowered(true);
        }
        else if (item is TPLinkSmartBulb bulb) 
        {
            bulb.SetPoweredOn(true);
        }
    }
    
### Remote Control

If you still want to control your devices remotely (not from within the same network) there is the possibility to link each device independently to your kasa account. It then shows up in your app.
```
smartdevice.ConfigureRemoteAccess("username", "password");
```
To check if your device is linked to tplink cloud use `RemoteAccessEnabled` property.

## Changelog

### [1.0.2] - 2020-04-17

#### Added

- associate devices to home network
- configure remote access via tplink cloud and kasa

#### Changed
- more consistent asynchronous code -> all commands to smart devices are now asynchronous
- restructured parts of code for better readability

## Disclaimer
I can not guarantee the functionality of this library as I only tested a HS100 and a KL130 in a Xamarin.Android application yet.

This library has no affiliation with TP-Link.
TP-Link and all respective product names are copyright TP-Link Technologies Co, Ltd. and/or its subsidiaries and affiliates.
