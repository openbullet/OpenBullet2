FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq curl git nano
RUN curl -sL https://deb.nodesource.com/setup_current.x | bash - && apt-get install -yq nodejs build-essential
RUN mkdir /app
WORKDIR /app
RUN apt-get install wget unzip -y
RUN wget $(curl -s https://api.github.com/repos/openbullet/OpenBullet2/releases/latest | grep 'browser_' | cut -d\" -f4)
RUN unzip OpenBullet2.zip
RUN cp -r ./OpenBullet2/* . && rm -rf ./OpenBullet2 && rm OpenBullet2.zip
EXPOSE 5000
CMD ["dotnet", "./OpenBullet2.dll", "--urls=http://*:5000"]