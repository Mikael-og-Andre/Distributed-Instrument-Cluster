#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Blazor Instrument Cluster/Server/Blazor Instrument Cluster.Server.csproj", "Blazor Instrument Cluster/Server/"]
COPY ["Instrument Communicator Library/Server Library.csproj", "Instrument Communicator Library/"]
COPY ["NetworkingLibrary/Networking Library.csproj", "NetworkingLibrary/"]
COPY ["Blazor Instrument Cluster/Client/Blazor Instrument Cluster.Client.csproj", "Blazor Instrument Cluster/Client/"]
COPY ["Blazor Instrument Cluster/Shared/Blazor Instrument Cluster.Shared.csproj", "Blazor Instrument Cluster/Shared/"]
COPY ["PackageClasses/PackageClasses.csproj", "PackageClasses/"]
COPY ["Video Library/Video Library.csproj", "Video Library/"]
RUN dotnet restore "Blazor Instrument Cluster/Server/Blazor Instrument Cluster.Server.csproj"
COPY . .
WORKDIR "/src/Blazor Instrument Cluster/Server"
RUN dotnet build "Blazor Instrument Cluster.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blazor Instrument Cluster.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blazor_Instrument_Cluster.Server.dll"]