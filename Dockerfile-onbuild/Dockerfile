FROM microsoft/dotnet:1.0.0-preview1

RUN cd / \
 && git clone https://github.com/darkcrash/bjd5.git \
 && cd /bjd5 \
 && rm -fr *.Test \
 && rm -fr *.sln \
 && rm -fr *.sh \
 && dotnet restore

WORKDIR /bjd5/Bjd.CoreCLR

EXPOSE 110  1080 5050 5060 8021 8023 8025 8080 8090 8110

ENTRYPOINT ["dotnet", "run"]