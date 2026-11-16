# One Dockerfile for every service: the compose file chooses which project to publish.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo
COPY Directory.Build.props Directory.Packages.props ./
COPY src ./src
ARG PROJECT
# sharing=locked: the services build in parallel and must not write the NuGet cache at the same time.
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "src/${PROJECT}" -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ARG ASSEMBLY
ENV ASSEMBLY=${ASSEMBLY} \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet ${ASSEMBLY}.dll"]
