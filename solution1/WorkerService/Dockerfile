FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /app
COPY WorkerService.Contracts/*.csproj ./WorkerService.Contracts/
COPY Common/*.csproj ./Common/
COPY WorkerService/*.csproj ./WorkerService/
WORKDIR /app/WorkerService
RUN dotnet restore

COPY ./WorkerService.Contracts /app/WorkerService.Contracts/
COPY ./Common /app/Common/
COPY ./WorkerService /app/WorkerService/
RUN dotnet publish -c Release -maxCpuCount -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "WorkerService.dll"]