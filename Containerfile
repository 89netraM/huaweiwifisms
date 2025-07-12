FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3da7c4198dc77b50aeaf76f262ed0ac2ed324f87fba4b5b0f0bc0b4fbbf2ad93 AS playwright

WORKDIR /app

COPY ./global.json ./Directory.Packages.props ./

RUN dotnet new console --output .
RUN dotnet add package Microsoft.Playwright
RUN dotnet build
RUN ./bin/Debug/net9.0/playwright.ps1 install chromium

FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:7269109eb94f0f63cb99179a032d697ee06e5873901b7cd611bcba5553257558 AS base

RUN apt-get update && apt-get install -y --no-install-recommends libasound2 libatk-bridge2.0-0 libatk1.0-0 libatspi2.0-0 libcairo2 libcups2 libdbus-1-3 libdrm2 libgbm1 libglib2.0-0 libnspr4 libnss3 libpango-1.0-0 libx11-6 libxcb1 libxcomposite1 libxdamage1 libxext6 libxfixes3 libxkbcommon0 libxrandr2 xvfb fonts-noto-color-emoji fonts-unifont libfontconfig1 libfreetype6 xfonts-scalable fonts-liberation fonts-ipafont-gothic fonts-wqy-zenhei fonts-tlwg-loma-otf fonts-freefont-ttf

COPY --from=playwright /root/.cache/ms-playwright /root/.cache/ms-playwright

FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:3da7c4198dc77b50aeaf76f262ed0ac2ed324f87fba4b5b0f0bc0b4fbbf2ad93 AS build

WORKDIR /app

COPY ./HuaweiWifiSms.slnx ./global.json ./Directory.Packages.props ./Directory.Build.props ./.gitignore ./.editorconfig ./

COPY ./HuaweiWifiSms/HuaweiWifiSms.csproj ./HuaweiWifiSms/packages.lock.json ./HuaweiWifiSms

RUN dotnet restore --locked-mode

COPY ./HuaweiWifiSms ./HuaweiWifiSms

RUN dotnet build ./HuaweiWifiSms/HuaweiWifiSms.csproj --no-restore --configuration Release

RUN dotnet publish ./HuaweiWifiSms/HuaweiWifiSms.csproj --no-build --configuration Release --output /out

FROM base AS release

WORKDIR /app

COPY --from=build /out ./

ENTRYPOINT ["./HuaweiWifiSms"]
