FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out
RUN rm /app/opus.dll

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=build-env /app/out .
RUN apk add --update \
        && apk add --no-cache --virtual .build bash git curl wget build-base g++ opusfile libopusenc opus libsodium youtube-dl ffmpeg

ENTRYPOINT ["dotnet", "Ranka.dll"]
