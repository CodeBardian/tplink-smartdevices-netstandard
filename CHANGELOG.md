## Changelog

### [2.0.1] - 2022-07-29

#### Added

- automatic push to nuget on release ([#dafb5be](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commit/dafb5be))

#### Fixed
- exception on sending commands to multiplugs ([#18](https://github.com/CodeBardian/tplink-smartdevices-netstandard/pull/18))

### [2.0.0] - 2021-04-04

#### Added

- support for hs220 dimmer switch ([#28556c9](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/28556c9))
- Timers
- Schedules

#### Changed
- no more `Task.Run` ([#1986900](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/1986900))

#### Fixed
- exception on retrieving cloud info without internet connection([#96be351e](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/6be351e))

### [1.0.4] - 2020-11-16

#### Added

- light details of smart bulbs ([#af850dd](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/af850dd))
- apply the four preset light states of smart bulbs ([#03e83c9](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/03e83c9) + [#e68bed7](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/e68bed7))
- transition time between light states for smart bulbs ([#db66403](https://github.com/CodeBardian/tplink-smartdevices-netstandard/commits/db66403))
- support for multi-outlet plugs (HS300, HS107, KP303?)
- ConfigureAwait(false) on awaiting tasks ([#16](https://github.com/CodeBardian/tplink-smartdevices-netstandard/pull/16))

#### Changed
- avoid blocking in async methods ([#16](https://github.com/CodeBardian/tplink-smartdevices-netstandard/pull/16))
- discovery now accepts broadcast address parameter ([#15](https://github.com/CodeBardian/tplink-smartdevices-netstandard/pull/15))

#### Fixed
- color changing of kl-130 model ([#9](https://github.com/CodeBardian/tplink-smartdevices-netstandard/pull/16))
- exception on discovering kl-130 model
- exception on energy stat parsing ([#12](https://github.com/CodeBardian/tplink-smartdevices-netstandard/pull/12))

### [1.0.3] - 2020-08-08

#### Added

- access energy stats of hs110 
- handle different hardware versions for hs110 

#### Changed
- project now targets .net standard 2.0
- improved emetering commands with error handling
- improved XML documentation for IntelliSense recommendations

#### Fixed
- location is no longer of type integer
- udp port correctly closing

### [1.0.2] - 2020-04-17

#### Added

- associate devices to home network
- configure remote access via tplink cloud and kasa

#### Changed
- more consistent asynchronous code -> all commands to smart devices are now asynchronous
- restructured parts of code for better readability
