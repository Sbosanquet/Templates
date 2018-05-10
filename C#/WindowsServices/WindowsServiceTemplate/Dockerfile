FROM microsoft/dotnet-framework:4.7.1-windowsservercore-1709
# Required for visual studio to build the images
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["WindowsServiceTemplate.exe"]
CMD ["-console"]
# 
#WORKDIR /Debug
#COPY bin/Debug .
#ENTRYPOINT ["c:\\windows\\system32\\cmd.exe", "/Q /C c:\\Debug\\WindowsServiceTemplate.exe -console"]