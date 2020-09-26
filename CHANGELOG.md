## Changelog

### [1.0.4] - unreleased

#### Added

- light details of smart bulbs

#### Changed

#### Fixed
- color changing of kl-130 model
- exception on discovering kl-130 model

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
