BlackJumboDog
=============

[Apache License Version 2.0](LICENSE)

## Build Status
[![blackjumbodog-core MyGet Build Status](https://www.myget.org/BuildSource/Badge/blackjumbodog-core?identifier=d28a64e2-3864-4cb0-b9b5-cf1a83cc77e8)](https://www.myget.org/)

## Future
* shift-jis be abolished. to utf-8.
* *.ini be abolished. to json.
* WebUI

## Requirement

* CGI - perl 


## Issues

* CGI not supported other than Windows


## TargetFramework
* NETStandard.Library 1.6
* Microsoft.NETCore.App 1.0.0
* .NET Core sdk 1.0.0-preview2-003121

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
cd Bjd.CoreCLR
dotnet run
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
 |-Option.ini
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


TAG

* darkcrash/blackjumbodog-dotnet-core:latest-run
 * ` dotnet restore ` ` dotnet run `

* darkcrash/blackjumbodog-dotnet-core:latest
 * ` dotnet publish -c Relaese`


