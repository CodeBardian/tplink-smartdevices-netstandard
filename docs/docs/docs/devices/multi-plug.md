# TPLinkSmartMultiPlug Class
<small>Namespace: TPLinkSmartDevices.Devices</small><br/>
<small>Inheritance: TPLinkSmartDevice -> TPLinkSmartMultiPlug</small><br/><br/>
encloses plug specific system information and controls for plugs with multiple outlets

## Properties

### `AllOutletsPowered`
: Returns whether all the plugs outlet relais are powered on
``` csharp
public bool AllOutletsPowered { get; private set; }
```

### `Features`
: Returns the feature (capability) abbreviations for this plug
``` csharp
public string[] Features { get; private set; }
```

### `LedOn`
: If status led on smart plug is on 
``` csharp
public bool LedOn { get; private set; }
```

### `OutletCount`
: Returns number of outlets on this plug
``` csharp
public int OutletCount { get; private set; }
```

### `Outlets`
: Returns array of Outlets, containing id's, names and power states of each outlet
``` csharp
public Outlet[] Outlets { get; private set; }
```

## Constructors

### `TPLinkSmartMultiPlug(string, int)`
: Creates a new object of this type, used for HS300/HS107 plug 
  ``` csharp
  public TPLinkSmartPlug(string hostname, int port=9999)
  ```

    __Parameters__
    : * `#!csharp string hostname`: ip-address of of this plug
      * `#!csharp int port`: plug communicates on this port, defaults to `9999`

## Methods

### `Refresh()`
: Refreshes all properties of this plug (includes a call to [`TPLinkSmartDevice.Refresh(dynamic)`](device.md#refreshdynamic) for the common device information)
  ``` csharp
  public async Task Refresh()
  ```

### `SetOutletPowered(bool, int)`
: Change the plugs outlet relay state
  ``` csharp
  public void SetOutletPowered(bool value, int outledId = -1)
  ```

    __Parameters__
    : * `#!csharp bool value`: `true` power on, `false` power off
      * `#!csharp int outledId`: id of outlet to turn on/off (zero-based index of all outlets)

    __Exceptions__
    : * `#!csharp ArgumentException`: plug does not have a outlet with specified `outledId`

### `SetLedOn(bool)`
: Change the plugs LED state; branded as night mode by tp-link :)
  ``` csharp
  public void SetLedOn(bool value)
  ```

    __Parameters__
    : * `#!csharp bool value`: `true` LED on (day mode), `false` LED off (night mode)