dotnet restore
cd Bjd.CoreCLR
dotnet publish -c Release
mv bin/Release/netcoreapp1.0/publish ../
cd ../publish
dotnet Bjd.CoreCLR.dll