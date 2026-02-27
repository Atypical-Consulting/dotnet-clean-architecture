FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
RUN apk add --no-cache nodejs npm
WORKDIR /src
COPY ["src/WalletApp/WalletApp.csproj", "src/WalletApp/"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "src/ServiceDefaults/"]
RUN dotnet restore "src/WalletApp/WalletApp.csproj"
COPY . .
WORKDIR "/src/src/WalletApp"
RUN npm ci
RUN dotnet build "WalletApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WalletApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "WalletApp.dll"]
