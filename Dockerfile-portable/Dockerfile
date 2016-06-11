FROM microsoft/dotnet:1.0.0-preview1

RUN mkdir -p /dotnetapp
COPY . /dotnetapp
RUN cd /dotnetapp \
    && rm -fr *.Test \
    && rm -fr *.sln \
    && rm -fr *.sh \
    && rm -fr *.json 

ONBUILD RUN cd /dotnetapp
ONBUILD RUN ["dotnet", "restore"]
ONBUILD RUN cd /dotnetapp/Bjd.CoreCLR
ONBUILD RUN ["dotnet", "publish" ,"-c", "Release"]
ONBUILD RUN cd /dotnetapp
ONBUILD RUN mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish .

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