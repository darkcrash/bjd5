FROM microsoft/dotnet:1.0.0-preview1

RUN mkdir -p /dotnetapp
COPY . /dotnetapp
RUN cd /dotnetapp
RUN /usr/bin/dotnet restore
RUN cd /dotnetapp/Bjd.CoreCLR
RUN /usr/bin/dotnet publish
RUN cd /dotnetapp
RUN mv Bjd.CoreCLR Bjd
RUN rm -fr Bjd.*
RUN mv Bjd/bin/Debug/netcoreapp1.0/publish Bjd/bin/Debug/


WORKDIR /dotnetapp/Bjd/bin/Debug/publish/

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