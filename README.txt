Dell Update Tool C# WinForms

Build and run with the .NET SDK on Windows.

Quick start:
1. Open Command Prompt or PowerShell in this folder.
2. Run:
   dotnet build
3. Run:
   dotnet run

Publish a standalone EXE:
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

Published output will be under:
   bin\Release\net8.0-windows\win-x64\publish\
