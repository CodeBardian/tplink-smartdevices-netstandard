# PowerData Class
<small>Namespace: TPLinkSmartDevices.Data</small><br/><br/>
Encapsulates JSON data structure for current energy use as metered by the HS110 Smart Energy Meter.

## Properties

### `Voltage`
: Currently measured voltage in volts
``` csharp
public double Voltage { get; private set; }
```

### `Amperage`
: Currently measured current in amperes
``` csharp
public double Amperage { get; private set; }
```

### `Power`
: Currently measured power in watts
``` csharp
public double Power { get; private set; }
```

### `Power`
: Total power consumption in kilowatthours
``` csharp
public double Total { get; private set; }
```