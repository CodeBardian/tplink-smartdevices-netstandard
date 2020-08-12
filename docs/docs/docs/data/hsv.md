# BulbHSV Class
<small>Namespace: TPLinkSmartDevices.Data</small><br/><br/>
represents a single color in the HSV color model to change a smart bulbs color

## Properties

### `Hue`
: Angular dimension representing color, `0°/360°` red, `120°` green, `240°` blue
``` csharp
public byte Hue { get; set; }
```

### `Saturation`
: Resembles various tints of color
``` csharp
public byte Saturation { get; set; }
```

### `Value`
: Brightness of color (mixture of hue with varying amounts of black or white paint)
``` csharp
public byte Value { get; set; }
```