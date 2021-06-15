FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /jjj
COPY . ./
WORKDIR /jjj/JobsJobsJobs/Server
RUN dotnet publish JobsJobsJobs.Server.csproj -c Release /p:PublishProfile=Properties/PublishProfiles/FolderProfile.xml

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /jjj
COPY --from=build /jjj/JobsJobsJobs/Server/bin/Release/net5.0/linux-x64/publish/ ./
ENTRYPOINT [ "/jjj/JobsJobsJobs.Server" ]
