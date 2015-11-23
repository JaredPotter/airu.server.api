AirU Project: Server Application Programming Interface
=====================================

# About

The AirU Project consists of multiple components. This component is the Server API. There will be more details to come concerning its implementation.


HOW TO DEVELOP, CONFIGURE, AND DEPLOY THE SERVER API

1.) On Windows, use Git to download the entire airu.server.api source folder. 

2.) Open the solution file (.sln) inside the server_api folder with Visual Studio 2013 (Ultimate).

3.) Perform any code changes and test via the debugger.

4.) When ready to deploy, select the Build menu item and then Publish server_api.
-Publishing method: File System
-Target Location: ..(your path to repository)..\airu.server.api\server_api_deployment_package

5.) Use Git to upload both the code changes (server_api) and the deployment package to the dev branch of airu.server.api.

6.) On the server, use Git to pull down the previously uploaded files.

7.) If IIS is not set up to simply pull from the newly updated files, then create a new site in IIS and specify the deployment package folder as the target directory. Use your credentials to allow the server to access that folder. 

8.) Browse to api.airu.utah.edu/api/{controller} to validate functionality. 



# Requirements

<PLACEHOLDER> Here we will add our system requirements.
