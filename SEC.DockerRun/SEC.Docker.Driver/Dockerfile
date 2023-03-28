#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SEC.DockerRun/SEC.Docker.Driver/SEC.Docker.Driver.csproj", "SEC.DockerRun/SEC.Docker.Driver/"]
RUN dotnet restore "SEC.DockerRun/SEC.Docker.Driver/SEC.Docker.Driver.csproj"
COPY . .
WORKDIR "/src/SEC.DockerRun/SEC.Docker.Driver"
RUN dotnet build "SEC.Docker.Driver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SEC.Docker.Driver.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SEC.Docker.Driver.dll"]