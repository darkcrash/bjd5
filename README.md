BlackJumboDog
=============

[Apache License Version 2.0](LICENSE)


## future
* shift-jis be abolished. to utf-8.
* *.ini be abolished. to json.
* WebUI

## Requirement

* CGI - perl 


## Issues

* CGI not supported other than Windows


## TargetFramework
* Microsoft.NETCore.App 1.0.0-rc2-3002702

## deployments
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

## Docker Hub

[darkcrash/blackjumbodog-dotnet-core](https://hub.docker.com/r/darkcrash/blackjumbodog-dotnet-core/)


TAG

* darkcrash/blackjumbodog-dotnet-core:latest-onbuild

* darkcrash/blackjumbodog-dotnet-core


