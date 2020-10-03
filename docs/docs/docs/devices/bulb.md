# TPLinkSmartBulb Class
<small>Namespace: TPLinkSmartDevices.Devices</small><br/>
<small>Inheritance: TPLinkSmartDevice -> TPLinkSmartBulb</small><br/><br/>
encloses bulb specific system information and bulb controls

## Properties

### `IsColor`
: Returns whether bulb supports color changes
``` csharp
public bool IsColor { get; private set; }
```

### `IsDimmable`
: Returns whether bulb supports dimming the brightness
``` csharp
public bool IsDimmable { get; private set; }
```

### `IsVariableColorTemperature`
: Returns whether bulb supports changing of color temperature
``` csharp
public bool IsVariableColorTemperature { get; private set; }
```

### `Brightness`
: Returns bulb brightness in percent
``` csharp
public int Brightness { get; private set; }
```

### `ColorTemperature`
: Returns bulbs color temperature in kelvin
``` csharp
public int ColorTemperature { get; private set; }
```

### `LightDetails`
: Contains further hardware specifications of this bulb, , see [`LightDetails`](/docs/data/light-details.md) reference
``` csharp
public LightDetails LightDetails { get; private set; }
```

### `HSV`
: Returns bulb color in HSV scheme 
``` csharp
public BulbHSV HSV { get; private set; }
```

### `PoweredOn`
: Returns whether bulb is powered on 
``` csharp
public bool PoweredOn { get; private set; }
```

### `PreferredLightStates`
: Returns collection of the four light state preset configurations
``` csharp
public List<PreferredLightState> PreferredLightStates { get; }
```

## Constructors

### `TPLinkSmartBulb(string, int)`
: Creates a new object of this type, used for KL100/KL110/KL130 bulbs 
  ``` csharp
  public TPLinkSmartBulb(string hostname, int port=9999)
  ```

    __Parameters__
    : * `#!csharp string hostname`: ip-address of of this bulb
      * `#!csharp int port`: bulb communicates on this port, defaults to `9999`

## Methods

### `Refresh()`
: Refreshes all properties of this bulb (includes a call to [`TPLinkSmartDevice.Refresh(dynamic)`](device.md#refreshdynamic) for the common device information)
  ``` csharp
  public async Task Refresh()
  ```

!!! tip "Method is awaitable" 

### `SetPoweredOn(bool)`
: Change the power state of this bulb 
  ``` csharp
  public void SetPoweredOn(bool value)
  ```

    __Parameters__
    : * `#!csharp bool value`: `true` power on, `false` power off

### `SetBrightness(int, int)`
: Change the bulbs brightness
  ``` csharp
  public void SetBrightness(int brightness, int transition_period = 0)
  ```

    __Parameters__
    : * `#!csharp int brightness`: brightness value in percent
      * `#!csharp int transition_period` (optional): time in milliseconds in which the bulb transitions from old to new brightness. Allowed values between `0` and `10000`

    __Exceptions__
    : * `#!csharp NotSupportedException`: the bulb does not support dimming
      * `#!csharp ArgumentException`: `transition_period` only allows values between `0` and `10000`

### `SetColorTemp(int, int)`
: Change the bulbs color temperature
  ``` csharp
  public void SetColorTemp(int colortemp, int transition_period = 0)
  ```

    __Parameters__
    : * `#!csharp int colortemp`: color temperature in kelvin, common values ranging between 2700K (soft light) to 6500K (bright daylight)
      * `#!csharp int transition_period` (optional): time in milliseconds in which the bulb transitions from old to new brightness. Allowed values between `0` and `10000`

    __Exceptions__
    : * `#!csharp NotSupportedException`: the bulb does not support color temperature changes
      * `#!csharp ArgumentException`: `transition_period` only allows values between `0` and `10000`

!!! note 
    Color temperature values depend on device model, for instance KL120 supports 2500K-5000K and KL130 2700K-9000K!

### `SetColorTemp(BulbHSV, int)`
: Change the bulbs color 
  ``` csharp
  public void SetHSV(BulbHSV hsv, int transition_period = 0)
  ```

    __Parameters__
    : * `#!csharp BulbHSV hsv`: color in HSV color scheme, see [`BulbHSV`](/docs/data/hsv.md) reference
      * `#!csharp int transition_period` (optional): time in milliseconds in which the bulb transitions from old to new brightness. Allowed values between `0` and `10000`

    __Exceptions__
    : * `#!csharp NotSupportedException`: the bulb does not support color changes
      * `#!csharp ArgumentException`: `transition_period` only allows values between `0` and `10000`
   
### `ApplyPreset(int)`
: Operate smart bulb on one of the four light state presets
  ``` csharp
  public void ApplyPreset(int presetIndex)
  ```

    __Parameters__
    : * `#!csharp int presetIndex`: index of the four presets, ranging from `0` to `3`

    __Exceptions__
    : * `#!csharp ArgumentOutOfRangeException`: `presetIndex` only allows values between `0` and `3`
    