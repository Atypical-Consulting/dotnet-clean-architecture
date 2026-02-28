FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
RUN apk add --no-cache nodejs npm
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/CleanArchitecture.WalletApp/CleanArchitecture.WalletApp.csproj", "src/CleanArchitecture.WalletApp/"]
COPY ["src/CleanArchitecture.ServiceDefaults/CleanArchitecture.ServiceDefaults.csproj", "src/CleanArchitecture.ServiceDefaults/"]
RUN dotnet restore "src/CleanArchitecture.WalletApp/CleanArchitecture.WalletApp.csproj"
COPY . .
WORKDIR "/src/src/CleanArchitecture.WalletApp"
RUN npm ci
RUN dotnet build "CleanArchitecture.WalletApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CleanArchitecture.WalletApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "CleanArchitecture.WalletApp.dll"]
