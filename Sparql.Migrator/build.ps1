#!/usr/bin/env powershell
Set-Variable -Name "version" -Value "v0.1.0"

Remove-Item -Path .\obj -Recurse
Remove-Item -Path .\bin -Recurse

docker build -t sparqlmigrator:$version .
docker tag sparqlmigrator:$version sparqlmigrator:$version