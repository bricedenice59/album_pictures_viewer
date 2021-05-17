#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["MyStreamingApp.APIs/MyStreamingApp.APIs.csproj", "MyStreamingApp.APIs/"]
COPY ["EFSqliteDOTNET50.Utils/EFSqliteDOTNET50.Utils.csproj", "EFSqliteDOTNET50.Utils/"]
RUN dotnet restore "MyStreamingApp.APIs/MyStreamingApp.APIs.csproj"
COPY . .
WORKDIR "/src/MyStreamingApp.APIs"
RUN dotnet build "MyStreamingApp.APIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyStreamingApp.APIs.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyStreamingApp.APIs.dll"]
