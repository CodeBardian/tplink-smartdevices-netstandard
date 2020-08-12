# Full API Reference

## Classes

| TPLinkSmartDevice.Devices                 | Description                          |
| :---------------------------------------- | :----------------------------------- |
| [`TPLinkDiscovery`](discovery.md) | handles discovery of new smart devices and connection of factory new devices to a network  |
| [`TPLinkSmartDevice`](devices/device.md)  | provides top-level functionalities which all smart devices use, including set up of remote access and several system information properties|
| [`TPLinkSmartPlug`](devices/plug.md)      | encloses plug specific system information and plug controls |
| [`TPLinkSmartMeterPlug`](devices/smartmeter-plug.md) | provides data on power consumption of comsumers connected to a HS110 plug |
| [`TPLinkSmartBulb`](devices/bulb.md)      | encloses bulb specific system information and bulb controls |

| TPLinkSmartDevice.Data        | Description                          |
| :---------------------------- | :----------------------------------- |
| [`BulbHSV`](data/hsv.md)      | represents a single color in the HSV color model to change a smart bulbs color  |
| [`PowerData`](data/power.md)  | Encapsulates JSON data structure for current energy use as metered by the HS110 Smart Energy Meter |