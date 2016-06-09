BlackJumboDog
=============

[Apache License Version 2.0](LICENSE)


## future
* I make them move in CoreCLR.
* Remove GUI.
* shift-jis be abolished. to utf-8.
* *.ini be abolished. to json.
* deployment Docker.

## TargetFramework
* Microsoft.NETCore.App 1.0.0-rc2-3002702

## deployments
* Windows10
* Ubuntu (14.04)
* Docker
* osx

Shell on Ubuntu
```Bash:
git clone https://github.com/darkcrash/bjd5.git
cd bjd5
dotnet restore
cd Bjd.CoreCLR
dotnet run
```

## Dockerfile Pilot

[Dockerfile](Dockerfile)

```Dockerfile:Dockerfile
FROM microsoft/dotnet:onbuild

WORKDIR /dotnetapp/Bjd.CoreCLR

EXPOSE 110
EXPOSE 1080
EXPOSE 5050
EXPOSE 5060
EXPOSE 8021
EXPOSE 8023
EXPOSE 8025
EXPOSE 8080
EXPOSE 8090
EXPOSE 8110
```
