FROM darkcrash/dotnet:1.0.0-preview1

RUN mkdir -p /dotnetapp
COPY . /dotnetapp
RUN cd / \
 && cd /dotnetapp \
 && rm -fr *.Test \
 && rm -fr *.sln \
 && rm -fr *.sh \
 && dotnet restore

WORKDIR /dotnetapp/Bjd.CoreCLR

EXPOSE 110  1080 5050 5060 8021 8023 8025 8080 8090 8110

ENTRYPOINT ["dotnet", "run"]