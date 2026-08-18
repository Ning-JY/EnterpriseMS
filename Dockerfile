# =========================
# 第一阶段：编译
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish EnterpriseMS.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# 第二阶段：运行
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "EnterpriseMS.dll"]