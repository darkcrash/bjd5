FROM microsoft/dotnet:1.0.0-preview1

RUN mkdir -p /dotnetapp
COPY . /dotnetapp
RUN cd /dotnetapp \
    && rm -fr *.Test \
    && rm -fr *.sln \
    && rm -fr *.sh \
    && dotnet restore \
    && cd /dotnetapp/Bjd.CoreCLR \
    && dotnet publish -c Release \
    && cd /dotnetapp \
    && mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish .
    && rm -fr Bjd* \

WORKDIR /dotnetapp/publish/

ENTRYPOINT ["dotnet", "Bjd.CoreCLR.dll"]

EXPOSE 110  1080 5050 5060 8021 8023 8025 8080 8090 8110