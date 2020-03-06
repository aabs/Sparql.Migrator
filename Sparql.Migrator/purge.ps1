#!/usr/bin/env powershell

param (
    [string]$server = "https://ent-bkb-rel-neptune-master-cluster.cluster-cg5vfywesksh.ap-southeast-2.neptune.amazonaws.com:33107/sparql", 
    [string]$path = "C:\Users\pra.andrewm\source\repos\entity.basketball.relationships.seeddata\update-scripts"
)

docker run -v ${path}:/data --rm -it sparqlmigrator:v0.1.1 purge -s ${server}