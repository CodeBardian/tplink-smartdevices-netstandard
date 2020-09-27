# LightDetails Class
<small>Namespace: TPLinkSmartDevices.Data</small><br/><br/>
Encapsulates JSON data structure for specific hardware properties of smart bulbs.

## Properties

### `ColorRenderingIndex`
: Measurement of how true the color of an object looks under the bulb's light. A good CRI for most indoor residential applications is 80 or above
``` csharp
public int ColorRenderingIndex { get; set; }
```

### `IncandescentEquivalent`
: bulb equals a standard incandescent bulb with this watt value
``` csharp
public int IncandescentEquivalent { get; set; }
```

### `LampBeamAngle`
: Angle at which the light is distributed or emitted
``` csharp
public int LampBeamAngle { get; set; }
```

### `MaxLumens`
: maximum brightness of bulb in lumens
``` csharp
public int MaxLumens { get; set; }

### `MaxVoltage`
: maximum operating voltage
``` csharp
public int MaxVoltage { get; set; }

### `MinVoltage`
: minimum operating voltage 
``` csharp
public int MinVoltage { get; set; }
```

### `Wattage`
: energy usage of bulb in watt
``` csharp
public int Wattage { get; set; }