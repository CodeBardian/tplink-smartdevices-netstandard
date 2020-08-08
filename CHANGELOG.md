## Changelog

### [1.0.3] - unreleased

#### Added

- access energy stats of hs100 

#### Changed
- project now targets .net standard 2.0
- improved emetering commands with error handling

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
