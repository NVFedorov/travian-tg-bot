FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY TravianTelegramBot/TravianTelegramBot.csproj TravianTelegramBot/
COPY TTB.Common/TTB.Common.csproj TTB.Common/
COPY TTB.Gameplay/TTB.Gameplay.csproj TTB.Gameplay/
COPY TTB.DAL/TTB.DAL.csproj TTB.DAL/
RUN dotnet restore TravianTelegramBot/TravianTelegramBot.csproj
COPY . .
WORKDIR /src/TravianTelegramBot
RUN dotnet build TravianTelegramBot.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish TravianTelegramBot.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TravianTelegramBot.dll"]
