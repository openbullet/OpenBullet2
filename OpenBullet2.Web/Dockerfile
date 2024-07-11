# -------
# BACKEND
# -------
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS backend

WORKDIR /code

COPY . .

#Disable Beauty to avoid Errors
RUN sed -i 's/<DisableBeauty>False<\/DisableBeauty>/<DisableBeauty>True<\/DisableBeauty>/' OpenBullet2.Web/OpenBullet2.Web.csproj

RUN dotnet publish OpenBullet2.Web -c Release -o /build

WORKDIR /build

# Remove all .xml files
RUN find . -name "*.xml" -type f -delete

# Manually copy over the dbip-country-lite.mmdb file from /code to /build
# since for some reason it doesn't get copied over by the dotnet publish command
RUN cp /code/OpenBullet2.Web/dbip-country-lite.mmdb /build

# --------
# FRONTEND
# --------
FROM node:20.3.0 AS frontend

WORKDIR /code

COPY openbullet2-web-client/package.json .
COPY openbullet2-web-client/package-lock.json .
RUN npm install

COPY openbullet2-web-client .
RUN npm run build
RUN mkdir /build && mv dist/* /build

# ---
# RUN
# ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim

WORKDIR /app

# TODO: Add other dependencies (e.g. node, python, etc.)
RUN apt-get update -yq && apt-get install -y --no-install-recommends curl

COPY --from=backend /build .
COPY --from=frontend /build ./wwwroot
COPY OpenBullet2.Web/dbip-country-lite.mmdb .

# Create a text file with all the files and folders in the current directory, not recursive, to be used by the updater
RUN ls -1 > build-files.txt

EXPOSE 5000

CMD ["dotnet", "./OpenBullet2.Web.dll", "--urls=http://*:5000"]
