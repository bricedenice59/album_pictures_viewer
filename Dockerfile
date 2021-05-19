#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
RUN apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["PhotoApp.APIs/PhotoApp.APIs.csproj", "PhotoApp.APIs/"]
COPY ["PhotoApp.Utils/PhotoApp.Utils.csproj", "PhotoApp.Utils/"]
COPY ["PhotoApp.Db/PhotoApp.Db.csproj", "PhotoApp.Db/"]
RUN dotnet restore "PhotoApp.APIs/PhotoApp.APIs.csproj"
COPY . .
WORKDIR "/src/PhotoApp.APIs"
RUN dotnet build "PhotoApp.APIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoApp.APIs.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoApp.APIs.dll"]
