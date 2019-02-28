# ASP.NET MVC Microsoft Graph: List OneDrive contents

## Introduction

List all OneDrive files and folders and write a log file which can be opened as a csv file.

## Getting Started

Get your app ID (client ID) and secret from [Microsoft Graph Quick Start](https://developer.microsoft.com/en-us/graph/quick-start) and download their code sample. You can find these settings in the file 'graph-tutorial/PrivateSettings.config'.

Download this Visual Studio solution (graph-tutorial-onedrive), open it and overwrite `ida:AppID` and `ida:AppSecret` settings in 'PrivateSettings.config' with your app ID and secret.

## Build and Test

Open the solution with Visual Studio and debug.
Sign in and click the Drive menu option. The root contents of your OneDrive will be displayed and the full list of folders and files will be written in App_Data.

## Acknowledgement

[Microsoft Graph tutorials](https://docs.microsoft.com/en-us/graph/tutorials)
