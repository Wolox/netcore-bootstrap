FROM microsoft/dotnet:2.0-sdk
WORKDIR /app
EXPOSE 80

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY src/*.csproj ./src/
COPY test/*.csproj ./test/
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
RUN dotnet restore

# Copy and build everything else
COPY . ./
