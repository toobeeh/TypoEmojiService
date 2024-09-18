FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

# grpc
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tobeh.TypoEmojiService.Server.csproj", "tobeh.TypoEmojiService.Server/"]
RUN dotnet restore "tobeh.TypoEmojiService.Server/tobeh.TypoEmojiService.Server.csproj"
COPY . ./tobeh.TypoEmojiService.Server
WORKDIR "/src/tobeh.TypoEmojiService.Server"
#RUN ls -lR
RUN dotnet build "tobeh.TypoEmojiService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "tobeh.TypoEmojiService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD ["sh", "-c", "dotnet tobeh.TypoEmojiService.Server.dll"]