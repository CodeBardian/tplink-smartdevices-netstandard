# tplink-smartdevices-netstandard
.NET Standard 1.6 Library for Discovering and Operating TP-Link Smart Devices 

This library allows a developer to discover and operate TP-Link Smart Devices with C# applications such as Xamarin, UWP or .net framework.
This includes support for TP-Link Smart Plugs HS100/105/110 as well as TP-Link Smart Bulbs KL/LB: 100/110/120/130.

This project is migrated to .net standard from Anthony Turner's TP-Link Smart Devices SDK: <br>
https://github.com/anthturner/TPLinkSmartDevices <br>
some minor changes have been made, e.g added asynchronous code, support of newer KL-series bulbs

Consult https://github.com/dotnet/standard/blob/master/docs/versions.md to see which .net platform versions can implement this library before using!
#### Prerequisites
Before using tplink-smartdevices your devices must be connected to the Wi-Fi network.
This can be done using the TP-Link provided mobile app Kasa.

## Usage
Use NuGet package manager to add a reference to this project, for example with dotnet cli:
```
> dotnet add package tplink-smartdevices --version 1.0.0
```
### Discovery
	// Runs in a async Task<List<XamarinTPLinkSmartDevice>>
	var discoveredDevices = await new TPLinkDiscovery().Discover();


### Example Usage
    var smartPlug = new TPLinkSmartPlug("100.10.4.1");
    smartPlug.OutletPowered = true; // Turn on relay
    smartPlug.OutletPowered = false; // Turn off relay

    var smartBulb = new TPLinkSmartBulb("100.10.4.2");
    smartBulb.PoweredOn = true; // Turn on bulb
    smartBulb.PoweredOn = false; // Turn off bulb
 
or after discovery:
    
    foreach (var item in discoveredDevices)
    {
        if (item is TPLinkSmartPlug plug)
        {
            plug.OutletPowered = true;
        }
        else if (item is TPLinkSmartBulb bulb) 
        {
            bulb.PoweredOn = true;
        }
    }

## Disclaimer
I can not guarantee the functionality of this library as I only tested a HS100 and a KL130 in a Xamarin.Android application yet.

This library has no affiliation with TP-Link.
TP-Link and all respective product names are copyright TP-Link Technologies Co, Ltd. and/or its subsidiaries and affiliates.
