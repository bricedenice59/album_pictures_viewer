#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["PhotoApp.Web/PhotoApp.Web.csproj", "PhotoApp.Web/"]
RUN dotnet restore "PhotoApp.Web/PhotoApp.Web.csproj"
COPY . .
WORKDIR "/src/PhotoApp.Web"
RUN dotnet build "PhotoApp.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoApp.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoApp.Web.dll"]