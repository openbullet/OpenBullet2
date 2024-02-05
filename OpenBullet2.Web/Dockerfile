# BACKEND
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS backend

WORKDIR /code

COPY . .
RUN dotnet publish OpenBullet2.Web -c Release -r win-x64 -o /build

# FRONTEND
FROM node:20.3.0 AS frontend

WORKDIR /code

COPY openbullet2-web-client/package.json .
COPY openbullet2-web-client/package-lock.json .
RUN npm install

COPY openbullet2-web-client .
RUN npm run build
RUN mkdir /build && mv dist/* /build

# RUN
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim

WORKDIR /app

# TODO: Add other dependencies
RUN apt-get update -yq && apt-get install -y --no-install-recommends curl

COPY --from=backend /build .
COPY --from=frontend /build ./wwwroot

EXPOSE 5000

CMD ["dotnet", "./OpenBullet2.Web.dll", "--urls=http://*:5000"]
