FROM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build-env
WORKDIR /app
COPY . ./
COPY nuget.config ./

RUN dotnet restore "Daimler.Providence.Service/Daimler.Providence.Service.csproj" --runtime linux-x64 --configfile ./nuget.config

RUN dotnet publish "Daimler.Providence.Service/Daimler.Providence.Service.csproj" -c Release  --no-restore --self-contained true --runtime linux-x64  --configfile ./nuget.config -o ./publish

FROM mcr.microsoft.com/dotnet/runtime:6.0-jammy AS runtime

ENV ASPNETCORE_URLS=http://+:8080

WORKDIR /app

COPY --from=build-env /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Daimler.Providence.Service.dll", "--urls", "http://*:8080"]