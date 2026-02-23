# This Dockerfile is meant to be run locally to build the OpenBullet2 project
# for normal usage via docker.

# -------
# BACKEND
# -------
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS backend

WORKDIR /code

COPY . .
RUN dotnet publish OpenBullet2.Web -c Release -o /build/web

WORKDIR /build/web

# Remove all .xml files
# (we cannot use <GenerateDocumentationFile>false</GenerateDocumentationFile>
# because we need it for swagger)
RUN find . -name "*.xml" -type f -delete

# Manually copy over the dbip-country-lite.mmdb file from /code to /build
# since for some reason it doesn't get copied over by the dotnet publish command
RUN cp /code/OpenBullet2.Web/dbip-country-lite.mmdb /build

# --------
# FRONTEND
# --------
FROM node:22-bookworm-slim AS frontend

WORKDIR /code

COPY openbullet2-web-client/package.json .
COPY openbullet2-web-client/package-lock.json .
RUN npm ci

COPY openbullet2-web-client .
RUN npm run build
RUN mkdir /build && mv dist/* /build

# ---------
# AGGREGATE
# ---------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim

ENV DEBIAN_FRONTEND=noninteractive

WORKDIR /app

COPY --from=backend /build/web .
COPY --from=frontend /build ./wwwroot
COPY OpenBullet2.Web/dbip-country-lite.mmdb .

# Install dependencies and Node.js in a single layer to reduce image size
RUN apt-get update -yq \
    && apt-get install -y --no-install-recommends \
        apt-utils curl git nano wget unzip \
        python3 \
        nodejs npm \
        chromium firefox-esr \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Note: Selenium 4.6+ includes Selenium Manager which automatically
# downloads the correct browser drivers at runtime. No need for
# the deprecated webdrivermanager pip package.

EXPOSE 5000
CMD ["dotnet", "./OpenBullet2.Web.dll", "--urls=http://*:5000"]
