#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNET_VERSION=5.0.5
ENV ASPNETCORE_URLS=http://+:80;https://+:443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Blazor_Instrument_Cluster/Server/Blazor_Instrument_Cluster.Server.csproj", "Blazor_Instrument_Cluster/Server/"]
COPY ["Blazor_Instrument_Cluster/Shared/Blazor_Instrument_Cluster.Shared.csproj", "Blazor_Instrument_Cluster/Shared/"]
COPY ["Packet_Classes/Packet_Classes.csproj", "Packet_Classes/"]
COPY ["Blazor_Instrument_Cluster/Client/Blazor_Instrument_Cluster.Client.csproj", "Blazor_Instrument_Cluster/Client/"]
COPY ["Server_Library/Server_Library.csproj", "Server_Library/"]
COPY ["Socket_Library/Socket_Library.csproj", "Socket_Library/"]
COPY ["Video Library/Video Library.csproj", "Video Library/"]
RUN dotnet restore "Blazor_Instrument_Cluster/Server/Blazor_Instrument_Cluster.Server.csproj"
COPY . .
WORKDIR "/src/Blazor_Instrument_Cluster/Server"
RUN dotnet build "Blazor_Instrument_Cluster.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blazor_Instrument_Cluster.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blazor_Instrument_Cluster.Server.dll"]