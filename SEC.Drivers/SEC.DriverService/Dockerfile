#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SEC.Drivers/SEC.DriverService/SEC.DriverService.csproj", "SEC.Drivers/SEC.DriverService/"]
RUN dotnet restore "SEC.Drivers/SEC.DriverService/SEC.DriverService.csproj"
COPY . .
WORKDIR "/src/SEC.Drivers/SEC.DriverService"
RUN dotnet build "SEC.DriverService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SEC.DriverService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SEC.DriverService.dll"]