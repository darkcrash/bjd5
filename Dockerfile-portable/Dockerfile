FROM darkcrash/blackjumbodog-dotnet-core:latest-run

RUN cd /dotnetapp/Bjd.CoreCLR \
 && dotnet publish -c Release \
 && cd /dotnetapp \
 && mv Bjd.CoreCLR/bin/Release/netcoreapp1.0/publish . \
 && rm -fr Bjd*

WORKDIR /dotnetapp/publish/

ENTRYPOINT ["dotnet", "Bjd.CoreCLR.dll"]

EXPOSE 1021 1025 1053 1067 1069 1080 1110 2000 2001 2002 2003 2004 2005 2006 2007 2008 2009 2010 2011 2012 2013 2014 2015 2016 2017 2018 2019 2020 5050 5060 8021 8023 8025 8080 8090 8110
