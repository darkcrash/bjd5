FROM microsoft/dotnet:1.0.0-preview1

RUN cd / \
 && git clone https://github.com/darkcrash/bjd5.git \
 && cd /bjd5 \
 && rm -fr *.Test \
 && rm -fr *.sln \
 && rm -fr *.sh \
 && dotnet restore \
 && cd /bjd5/Bjd.CoreCLR \
 && dotnet publish -c Release \
 && cd /bjd5 \
 && mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish . \
 && rm -fr Bjd*

WORKDIR /bjd5/publish/

ENTRYPOINT ["dotnet", "Bjd.CoreCLR.dll"]

EXPOSE 2010 2011 2012 2013 2014 2015 2016 2017 2018 2019 2020 2021 2022 2023 2024 2025 2026 2027 2028 2029 2030 3021 3025 3053 3067 3069 3080 3110 5050 5060 8021 8023 8025 8080 8090 8110
