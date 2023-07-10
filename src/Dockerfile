FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /jjj
COPY ./JobsJobsJobs.sln                                               ./
COPY ./JobsJobsJobs/Directory.Build.props                             ./JobsJobsJobs/
COPY ./JobsJobsJobs/Application/JobsJobsJobs.Application.fsproj       ./JobsJobsJobs/Application/
COPY ./JobsJobsJobs/Citizens/JobsJobsJobs.Citizens.fsproj             ./JobsJobsJobs/Citizens/
COPY ./JobsJobsJobs/Common/JobsJobsJobs.Common.fsproj                 ./JobsJobsJobs/Common/
COPY ./JobsJobsJobs/Home/JobsJobsJobs.Home.fsproj                     ./JobsJobsJobs/Home/
COPY ./JobsJobsJobs/Listings/JobsJobsJobs.Listings.fsproj             ./JobsJobsJobs/Listings/
COPY ./JobsJobsJobs/Profiles/JobsJobsJobs.Profiles.fsproj             ./JobsJobsJobs/Profiles/
COPY ./JobsJobsJobs/SuccessStories/JobsJobsJobs.SuccessStories.fsproj ./JobsJobsJobs/SuccessStories/
RUN dotnet restore

COPY . ./
WORKDIR /jjj/JobsJobsJobs/Application
RUN dotnet publish -c Release -r linux-x64
RUN rm bin/Release/net7.0/linux-x64/publish/appsettings.*.json

FROM  mcr.microsoft.com/dotnet/aspnet:7.0-alpine as final
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY --from=build /jjj/JobsJobsJobs/Application/bin/Release/net7.0/linux-x64/publish/ ./

EXPOSE 80
CMD [ "dotnet", "/app/JobsJobsJobs.Application.dll" ]
