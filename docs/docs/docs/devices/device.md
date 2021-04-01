# TPLinkSmartDevice Class
<small>Namespace: TPLinkSmartDevices.Devices</small><br/><br/>
provides top-level functionalities which all smart devices use, including set up of remote access and several system information properties

## Properties

### `Alias`
: Returns the user specified (or default) name of this device
``` csharp
public string Alias { get; private set; }
```

### `CloudServer`
: Returns the name of the server this device communicates to for cloud commands
``` csharp
public string CloudServer { get; private set; }
```

### `DeviceId`
: Returns the id of this device
``` csharp
public string DeviceId { get; private set; }
```

### `DevName`
: Returns the name of this device
``` csharp
public string DevName { get; private set; }
```

### `FirmwareId`
: Returns the firmware id of this device
``` csharp
public string FirmwareId { get; private set; }
```

### `HardwareId`
: Returns the hardware id of this device
``` csharp
public string HardwareId { get; private set; }
```

### `HardwareVersion`
: Returns the hardware version of this device
``` csharp
public string HardwareVersion { get; private set; }
```

### `Hostname`
: Returns the ip-address of this device
``` csharp
public string Hostname { get; private set; }
```

### `LocationLatLong`
: Returns the coordinates of the rough position the device is located at (location of network). `LocationLatLong[0]` is latitude, `LocationLatLong[1]` is longitude
``` csharp
public double[] LocationLatLong { get; private set; }
```

!!! caution 
    whether you find it questionable (I do!) or not, tp-link's devices collect data on position of your network. 


### `MacAddress`
: Returns the mac address of this device
``` csharp
public string MacAddress { get; private set; }
```

### `Model`
: Returns the model and region code (EU,US,UK,JP, ...) of this device
``` csharp
public string Model { get; private set; }
```

### `OemId`
: Returns the manufacturers id of this device
``` csharp
public string OemId { get; private set; }
```

### `Port`
: Returns the port this device communicates on
``` csharp
public int Port { get; private set; }
```

### `RemoteAccessEnabled`
: Returns whether this device is configured for remote access via Kasa app
``` csharp
public bool RemoteAccessEnabled { get; private set; }
```

### `RSSI`
: Returns signal strength 
``` csharp
public int RSSI { get; private set; } 
```

### `Type`
: 
``` csharp 
public string Type { get; private set; }
```

## Methods

### `ConfigureRemoteAccess(string, string)`
: Binds account with the specified credentials to tp-link's cloud server
  ``` csharp
  public async Task ConfigureRemoteAccess(string username, string password)
  ```

    __Parameters__
    : * `#!csharp string username`: username (e-mail address) of kasa account
      * `#!csharp string password`: password of kasa account

### `UnbindRemoteAccess()`
: Unbinds currently connected account from tp-link's cloud server
  ``` csharp
  public void UnbindRemoteAccess()
  ```

### `GetCloudInfo()`
: Refreshes cloud information and sets [`RemoteAccessEnabled`](#remoteaccessenabled) and [`CloudServer`](#cloudserver) properties accordingly
  ``` csharp
  public void GetCloudInfo()
  ```

### `GetTime()`
: Returns current internal time of this device 
  ``` csharp
  public DateTime GetTime()
  ```

!!! danger "Needs Maintenance" 
    This method needs maintenance. It is discouraged using it due to unexpected results or errors occurring

### `Refresh(dynamic)`
: Refreshes all properties of this device (includes a call to [`GetCloudInfo()`](#getcloudinfo))
  ``` csharp
  public async Task Refresh(dynamic sysInfo = null)
  ```

    __Parameters__
    : * `#!csharp dynamic sysInfo`: response of smart devices on system properties, defaults to `null` which results in a new request being made

### `SetAlias(string)`
: Sets alias of this device
  ``` csharp
  public void SetAlias(string value)
  ```

    __Parameters__
    : * `#!csharp string value`: new alias to set


<!-- ### `SetRemoteAccessEnabled(bool, string)`
: Enables/disables remote access by setting either tp-link's server or a bogus server to interrupt any communications to tp-link's cloud servers. 
  ``` csharp
  private void SetRemoteAccessEnabled(bool enabled, string server = "devs.tplinkcloud.com")
  ```

    __Parameters__
    : * `#!csharp bool enabled`: `true` enable, `false` disable remote access
      * `#!csharp string server`: server to receive cloud commands from, defaults to `devs.tplinkcloud.com`
 -->