BlackJumboDog
=============

[Apache License Version 2.0](LICENSE)

## Build Status

|   | Status |
|---|:-----:|
|MyGet pack|[![blackjumbodog-core MyGet Build Status](https://www.myget.org/BuildSource/Badge/blackjumbodog-core?identifier=d28a64e2-3864-4cb0-b9b5-cf1a83cc77e8)](https://www.myget.org/)|
|Packages|[![MyGet](https://img.shields.io/myget/blackjumbodog-core/v/Bjd.Common.CoreCLR.svg?maxAge=2592000?style=plastic)](https://www.myget.org/feed/Packages/blackjumbodog-core)|

## Test Status

| Projects | AppVeyor Project Status |
|---|:-----:|
|Bjd.Common.Test    |[![Build status](https://ci.appveyor.com/api/projects/status/p8i96er4tg3j8fbd?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5)|
|Bjd.DhcpServer.Test|[![Build status](https://ci.appveyor.com/api/projects/status/1ueg48oy4ha35qmv?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-vc4ju)|
|Bjd.DnsServer.Test |[![Build status](https://ci.appveyor.com/api/projects/status/xwv7m92c86qb3c85?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-3cwig)|
|Bjd.FtpServer.Test |[![Build status](https://ci.appveyor.com/api/projects/status/k10ucsh59xtyx7t9?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-5pd0r)|
|Bjd.Pop3Server.Test|[![Build status](https://ci.appveyor.com/api/projects/status/t0250q5wge4xuikt?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-hpn2n)|
|Bjd.SmtpServer.Test|[![Build status](https://ci.appveyor.com/api/projects/status/hv9pu705wpb5l7ri?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-qqab7)|
|Bjd.WebServer.Test |[![Build status](https://ci.appveyor.com/api/projects/status/8769awquopw95l59?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-qhoq4)|
|Bjd.Startup.Test   |[![Build status](https://ci.appveyor.com/api/projects/status/by5u3anq3g2gjb05?svg=true)](https://ci.appveyor.com/project/darkcrash/bjd5-p6o91)|

## Future
* shift-jis be abolished. to utf-8.
* *.ini be abolished. to json.
* WebUI

## Requirement

* CGI - perl 


## Issues

* CGI not supported other than Windows


## TargetFramework
* NETStandard.Library 1.6.1
* Microsoft.NETCore.App 1.1.0
* Microsoft .NET Core 1.1.1
* Microsoft .NET Core Tools for Visual Studio 2017

## Deployments
* Windows10
* Ubuntu (14.04)
* Docker
* osx


[install dotnet command](https://www.microsoft.com/net/core)

Shell on Ubuntu
```Bash:
git clone https://github.com/darkcrash/bjd5.git
cd bjd5
dotnet restore
cd Bjd.Startup
dotnet run
```

## Command Options

**It's possible to choose only one.**

* --interactive (defualt)

  interactive, using standard input and standard output.
  It's checked only by Windows.

* --console

  standard output log.
  for docker.

* --service

  standard output null.

Example --console
```Bash:
dotnet run --console
dotnet Bjd.Startup.dll --console
```
Example --service
```Bash:
dotnet run --service
dotnet Bjd.Startup.dll --service
```
Example --interactive
```Bash:
dotnet run
dotnet Bjd.Startup.dll
dotnet run --interactive
dotnet Bjd.Startup.dll --interactive
```




## Default Directory (dotnet publish)
```
publish
 |--logs
 |   |-dummy.txt
 |
 |--mailbox
 |   |-user
 |
 |--MailQueue
 |   |-dummy.txt
 |
 |--ProxyHttpCache
 |   |-dummy.txt
 |
 |--Tftp
 |   |-sample.txt
 |
 |--wwwroot
 |   |-index.html
 |   |-env.cgi
 |
 |-example.pfx
 |-named.ca
 |-Option.def
 |-Option.ini[* not used]
 |-Option.json
 |-Bjd.*.dll
 |
 |--runtimes
     |--linux
     |--osx.10.10
     |--unix
     |--win7


```

## Docker Hub

[darkcrash/blackjumbodog-dotnet-core](https://hub.docker.com/r/darkcrash/blackjumbodog-dotnet-core/)

[![Docker Stars](https://img.shields.io/docker/stars/darkcrash/blackjumbodog-dotnet-core.svg?maxAge=2592000?style=plastic)](https://hub.docker.com/r/darkcrash/blackjumbodog-dotnet-core/)
[![Docker Pulls](https://img.shields.io/docker/pulls/darkcrash/blackjumbodog-dotnet-core.svg?maxAge=2592000?style=plastic)](https://hub.docker.com/r/darkcrash/blackjumbodog-dotnet-core/)

TAG

* darkcrash/blackjumbodog-dotnet-core:latest-run
 * ` dotnet restore ` ` dotnet run --console`

* darkcrash/blackjumbodog-dotnet-core:latest
 * ` dotnet publish -c Relaese`


