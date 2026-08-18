FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY EnterpriseMS.csproj ./

RUN dotnet restore

COPY . .

RUN dotnet publish EnterpriseMS.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "EnterpriseMS.dll"]