# TPLinkSmartDimmmer Class
<small>Namespace: TPLinkSmartDevices.Devices</small><br/>
<small>Inheritance: TPLinkSmartDevice -> TPLinkSmartDimmer</small><br/><br/>
controls for wall switches with dimmer functionality 

## Properties

### `Brightness`
: Returns brightness (dimmer value) in percent
``` csharp
public int Brightness { get; private set; }
```

### `Options`
: Returns an object of type [`DimmerOptions`]() with configuration properties
``` csharp
public DimmerOptions Options { get; private set; }
```

### `Presets`
: Returns array of the four dimmer state preset configurations containing brightness values
``` csharp
public int[] Presets { get; private set; }
```

### `PoweredOn`
: Returns whether switch is powered on 
``` csharp
public bool PoweredOn { get; private set; }
```

## Constructors

### `TPLinkSmartPlug(string, int, DimmerOptions)`
: Creates a new object of this type, used for HS220 wall switch
  ``` csharp
  public TPLinkSmartPlug(string hostname, int port=9999, DimmerOptions opts = null)
  ```

    __Parameters__
    : * `#!csharp string hostname`: ip-address of this device
      * `#!csharp int port`: device communicates on this port, defaults to `9999`
      * `#!csharp DimmerOptions opts`: configuration properties`

## Methods

### `Create(string, int, DimmerOptions)` {: #create }
: Factory instantiation method. Returns a new instance of this type.
  ``` csharp
  public static async Task<TPLinkSmartPlug> Create(string hostname, int port = 9999, DimmerOptions opts = null)
  ```

    __Parameters__
    : * `#!csharp string hostname`: ip-address of this device
      * `#!csharp int port`: device communicates on this port, defaults to `9999`
      * `#!csharp DimmerOptions opts`: configuration properties`

### `Refresh()`
: Refreshes all properties of this device (includes a call to [`TPLinkSmartDevice.Refresh(dynamic)`](device.md#refreshdynamic) for the common device information)
  ``` csharp
  public async Task Refresh()
  ```

### `SetPoweredOn(bool)`
: Change the power state 
  ``` csharp
  public void SetPoweredOn(bool value)
  ```

    __Parameters__
    : * `#!csharp bool value`: `true` power on, `false` power off

### `TransitionBrightness(int, DimmerMode, int)`
: Transition to a specified brightness level
  ``` csharp
  public async Task TransitionBrightness(int brightness, DimmerMode? mode = null, int? duration = null)
  ```

    __Parameters__
    : * `#!csharp int brightness`: dimmer brightness value in percent
      * `#!csharp DimmerMode mode` (optional): [`DimmerMode`]() to use during this transition, if left empty uses default option from [`Options.Mode`](#options)
      * `#!csharp int duration` (optional): time in milliseconds in which the bulb transitions from old to new brightness

    __Exceptions__
    : * `#!csharp ArgumentException`: `brightness` should be between `0` and `100`

### `SetBrightness(int)`
: Instantly change to a specified brightness level
  ``` csharp
  public async Task SetBrightness(int brightness)
  ```

    __Parameters__
    : * `#!csharp int brightness`: dimmer brightness value in percent

    __Exceptions__
    : * `#!csharp ArgumentException`: `brightness` should be between `0` and `100`

### `SetDoubleClickAction(DimmerMode, int)`
: Configures change mode on double click of switch
  ``` csharp
  public async Task SetDoubleClickAction(DimmerMode mode, int index=0)
  ```

    __Parameters__
    : * `#!csharp DimmerMode mode`: [`DimmerMode`]() to use on double clicking the switch
      * `#!csharp int index` (optional): zero-based preset index, use in combination with `DimmerMode.Preset` to execute preset on double click

    __Exceptions__
    : * `#!csharp ArgumentException`: `index` should be between `0` and `3`

### `SetLongPressAction(DimmerMode, int)`
: Configures change mode on long press of switch
  ``` csharp
  public async Task SetLongPressAction(DimmerMode mode, int index=0)
  ```

    __Parameters__
    : * `#!csharp DimmerMode mode`: [`DimmerMode`]() to use on long press of switch
      * `#!csharp int index` (optional): zero-based preset index, use in combination with `DimmerMode.Preset` to execute preset on long press

    __Exceptions__
    : * `#!csharp ArgumentException`: `index` should be between `0` and `3`

### `SetFadeOnTime(int)`
: Configures speed of fade on transition
  ``` csharp
  public async Task SetFadeOnTime(int fadeOnTime)
  ```

    __Parameters__
    : * `#!csharp int fadeOnTime`: transition time used on next uses of switch when turning on

    __Exceptions__
    : * `#!csharp ArgumentException`: `fadeOnTime` should be a positive number

### `SetFadeOffTime(int)`
: Configures speed of fade on transition
  ``` csharp
  public async Task SetFadeOffTime(int fadeOffTime)
  ```

    __Parameters__
    : * `#!csharp int fadeOffTime`: transition time used on next uses of switch when turning off

    __Exceptions__
    : * `#!csharp ArgumentException`: `fadeOffTime` should be a positive number
