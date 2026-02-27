FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN dotnet tool install --global dotnet-ef --version 10.0.*
WORKDIR /src
COPY . .
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN dotnet restore "src/WebApi/WebApi.csproj"
RUN dotnet build "src/WebApi/WebApi.csproj" --no-restore
