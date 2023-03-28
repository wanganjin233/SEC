#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CEE.Operation/CEE.ChuangFeng/CEE.ChuangFeng.csproj", "CEE.Operation/CEE.ChuangFeng/"]
RUN dotnet restore "CEE.Operation/CEE.ChuangFeng/CEE.ChuangFeng.csproj"
COPY . .
WORKDIR "/src/CEE.Operation/CEE.ChuangFeng"
RUN dotnet build "CEE.ChuangFeng.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CEE.ChuangFeng.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CEE.ChuangFeng.dll"]