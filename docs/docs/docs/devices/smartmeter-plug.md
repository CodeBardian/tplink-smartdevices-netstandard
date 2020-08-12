# TPLinkSmartMeterPlug Class
<small>Namespace: TPLinkSmartDevices.Devices</small><br/>
<small>Inheritance: TPLinkSmartPlug -> TPLinkSmartMeterPlug</small><br/><br/>
provides data on power consumption of comsumers connected to a HS110 plug

## Properties

### `CurrentPowerUsage`
: Returns a [`PowerData`](/docs/data/power) object including power usage data from the latest call to the construtor or [`.Refresh()`](#refresh)
``` csharp
public PowerData CurrentPowerUsage { get; private set; }
```

### `IGain`
: Returns ratio of output current to input current.
``` csharp
public uint IGain { get; private set; }
```

### `VGain`
: Returns ratio of output voltage to input voltage.
``` csharp
public uint VGain { get; private set; }
```

## Constructors

### `TPLinkSmartMeterPlug(string)`
: Creates a new object of this type, used for HS110 plug 
  ``` csharp
  public TPLinkSmartMeterPlug(string hostname)
  ```

    __Parameters__
    : * `#!csharp string hostname`: ip-address of of this plug

## Methods

### `Refresh()`
: Updates current power usage, gain data and all other properties of this plug (includes a call to [`TPLinkSmartPlug.Refresh()`](plug.md#refresh) for the common device information)
  ``` csharp
  public async Task Refresh()
  ```

!!! tip "Method is awaitable" 

### `EraseStats()`
: Erases all collected e-meter statistics of this plug
  ``` csharp
  public void EraseStats()
  ```


### `GetMonthStats(int, int)`
: Queries collected usage statistics from a specific month. Returns a `#!csharp Dictionary<DateTime, int>` of each day in a month and energy consumption of that day (in watt hours (?))
  ``` csharp
  public async Task<Dictionary<DateTime, int>> GetMonthStats(int month, int year)
  ```

    __Parameters__
    : * `#!csharp int month`: month of year, ranging from `1`(January) to `12`(December)
      * `#!csharp int year`: 

!!! tip "Method is awaitable" 

### `GetYearStats(int)`
: Queries collected usage statistics for a whole year. Returns a `#!csharp Dictionary<int, int>` of each month and energy consumption of month (in watt hours (?))
  ``` csharp
  public async Task<Dictionary<int, int>> GetYearStats(int year)
  ```

    __Parameters__
    : * `#!csharp int year`: 

!!! tip "Method is awaitable" 