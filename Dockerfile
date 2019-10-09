FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /NetCoreBootstrap.Api
COPY . ./
RUN dotnet restore
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o /app
CMD ASPNETCORE_URLS=http://*:$PORT dotnet NetCoreBootstrap.Api.dll
