FROM microsoft/dotnet:2.0.3-sdk AS builder
WORKDIR /app
EXPOSE 80

# Copy csproj and restore as distinct layers
# COPY *.sln ./
# COPY src/*.csproj ./src/
# COPY test/*.csproj ./test/
COPY . .

RUN dotnet restore -c Release
RUN dotnet build -c Release --no-restore  
RUN dotnet pack -c Release --no-restore --no-build 
