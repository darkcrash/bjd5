FROM microsoft/dotnet:1.0.0-preview1

RUN mkdir -p /dotnetapp
COPY . /dotnetapp
RUN cd /dotnetapp \
    && rm -fr *.Test \
    && rm -fr *.sln \
    && rm -fr *.sh \
    && rm -fr *.json \
    && cd /dotnetapp \
    && dotnet restore \
    && cd /dotnetapp/Bjd.CoreCLR \
    && dotnet publish -c Release \
    && cd /dotnetapp \
    &&  mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish .

WORKDIR /dotnetapp/publish/

ENTRYPOINT ["dotnet", "Bjd.CoreCLR.dll"]

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