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
FROM node:20.9.0 AS frontend

WORKDIR /code

COPY openbullet2-web-client/package.json .
COPY openbullet2-web-client/package-lock.json .
RUN npm install

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

# Install dependencies
RUN echo "deb http://deb.debian.org/debian/ unstable main contrib non-free" >> /etc/apt/sources.list.d/debian.list \
 && apt-get update -yq \
 && apt-get install -y --no-install-recommends \
    apt-utils \
 && apt-get upgrade -yq \
 && apt-get install -yq \
    curl \
    wget \
    unzip \
    git \
    python3 \
    python3-pip

# Setup nodejs
RUN curl -sL https://deb.nodesource.com/setup_current.x | bash - \
 && apt-get install -yq \
    nodejs \
    build-essential

# Install chromium and firefox for selenium and puppeteer
RUN apt-get update -yq && apt-get install -y --no-install-recommends firefox chromium \
 && pip3 install webdrivermanager || true \
 && webdrivermanager firefox chrome --linkpath /usr/local/bin || true

# Clean up
RUN apt-get remove curl wget unzip --yes \
&& apt-get clean autoclean --yes \
&& apt-get autoremove --yes \
&& rm -rf /var/cache/apt/archives* /var/lib/apt/lists/*

EXPOSE 5000
CMD ["dotnet", "./OpenBullet2.Web.dll", "--urls=http://*:5000"]
