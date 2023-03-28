#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 30000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SEC.DockerRun/SEC.Docker.Core/SEC.Docker.Core.csproj", "SEC.DockerRun/SEC.Docker.Core/"]
RUN dotnet restore "SEC.DockerRun/SEC.Docker.Core/SEC.Docker.Core.csproj"
COPY . .
WORKDIR "/src/SEC.DockerRun/SEC.Docker.Core"
RUN dotnet build "SEC.Docker.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SEC.Docker.Core.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SEC.Docker.Core.dll"]