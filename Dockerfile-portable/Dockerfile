FROM darkcrash/blackjumbodog-dotnet-core:latest-onbuild

RUN cd /dotnetapp/Bjd.CoreCLR \
 && dotnet publish -c Release \
 && cd /dotnetapp \
 && mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish . \
 && rm -fr Bjd*

WORKDIR /dotnetapp/publish/

ENTRYPOINT ["dotnet", "Bjd.CoreCLR.dll"]

EXPOSE 1021 1025 1053 1067 1069 1080 1110 5050 5060 8021 8023 8025 8080 8090 8110
