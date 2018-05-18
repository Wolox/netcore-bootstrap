FROM microsoft/dotnet:2.0-sdk
WORKDIR /netapp
EXPOSE 80

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY src/*.csproj ./src/
COPY test/*.csproj ./test/
RUN dotnet restore

RUN chmod a+rw /netapp
