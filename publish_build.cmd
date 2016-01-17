nuget.exe restore server_api/server_api.sln
call "%VS120COMNTOOLS%\vsvars32.bat"
call msbuild server_api/server_api.sln /p:DeployOnBuild=true /p:PublishProfile=deployment_package /p:WebPublishMethod=FileSystem /p:publishUrl=C:\airu.server.api\server_api_deployment_package