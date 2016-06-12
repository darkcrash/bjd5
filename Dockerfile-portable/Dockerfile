FROM darkcrash/blackjumbodog-dotnet-core:latest-run

RUN cd /dotnetapp/Bjd.CoreCLR \
 && dotnet publish -c Release \
 && cd /dotnetapp \
 && mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish . \
 && rm -fr Bjd*

WORKDIR /dotnetapp/publish/

ENTRYPOINT ["dotnet", "Bjd.CoreCLR.dll"]

EXPOSE 2010 2011 2012 2013 2014 2015 2016 2017 2018 2019 2020 2021 2022 2023 2024 2025 2026 2027 2028 2029 2030 3021 3025 3053 3067 3069 3080 3110 5050 5060 8021 8023 8025 8080 8090 8110
