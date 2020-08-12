# TPLinkSmartPlug Class
<small>Namespace: TPLinkSmartDevices.Devices</small><br/>
<small>Inheritance: TPLinkSmartDevice -> TPLinkSmartPlug</small><br/><br/>
encloses plug specific system information and plug controls

## Properties

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

### `OutletPowered`
: Returns whether the plugs outlet relay is powered on
``` csharp
public bool OutletPowered { get; private set; }
```

### `PoweredOnSince`
: Returns `#!csharp DateTime` the relay was powered on
``` csharp
public DateTime PoweredOnSince { get; private set; }
```

## Constructors

### `TPLinkSmartPlug(string, int)`
: Creates a new object of this type, used for HS100/HS105 plug 
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

!!! tip "Method is awaitable" 

### `SetOutletPowered(bool)`
: Change the plugs outlet relay state
  ``` csharp
  public void SetOutletPowered(bool value)
  ```

    __Parameters__
    : * `#!csharp bool value`: `true` power on, `false` power off

### `SetLedOn(bool)`
: Change the plugs LED state; branded as night mode by tp-link :)
  ``` csharp
  public void SetLedOn(bool value)
  ```

    __Parameters__
    : * `#!csharp bool value`: `true` LED on (day mode), `false` LED off (night mode)