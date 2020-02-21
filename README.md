# SPARQL Migrator

SPARQL Migrator is a simple CLI app to control the application of SPARQL
migrations on a remote RDF Graph.

I works, just like every other similar tool, by maintaining a list of previously
applied migrations in the database it is maintaining.  In this case, it keeps a
named graph.  Every time it applies a SPARQL Update script to the triple store,
it adds a 'Migration' instance to the graph.  When the app is first run, it
downloads the list of every Migration previously run.  

As it cycles through all of the scripts it is able to find, it compares them
against the applied migrations.  If it finds a match, then it skips the script.
Comparison of scripts to preior migrations is on the basis of both name and Hash
Code.  If the name and hash code matches then the script is ignored.

The name is only compared on the base name (filename, not including the
directory or extension), not the full path, since updates might be applied from
different locations (such as different dev machines, or CI/CD boxes).  I
therefore suggest that you give the files a sensible naming convention that will
guarantee that they get applied in the order in which you create them.  

I prefer to use an ISO 8601 inspired format with precision down to the minute to
name my files.  `20200218T1200-drop.rq` is a good example.  Another alternative
that will work might just use sequential integers - but you have to remember to
do sufficient padding to ensure that later scripts don't get promoted ahead of
later ones.

## Format

It works by maintaining a Named Graph (url of
`<http://industrialinference.com/migrations/0.1#migrations>`) containing a set
of migration instances.

An instance typically looks like this:

```turtle
@prefix : <http://industrialinference.com/migrations/0.1#> .

:migrations {
	_:mig1 a :Migration ;
		:ordinal 1 ;
		:dtApplied "2020-02-19T13:45:22"^^xsd:dateTime ;
		:appliedBy "andrewm" ;
		:migrationHash "e52bbccd334751e803e43e2eb8f6ed5917528679e4b573c8ef925c30d9a6160f" ;
		:migratorVersion "0.1" ;
		:originalPath "/home/andrew/dev/Sparql.Migrator/test-data/migrations/20200218T1200-drop.rq" .
}
```

## Usage

Invoking the app without params will get you the usage options.

```shell
$ ./Sparql.Migrator.exe
Usage: Sparql.Migrator [options...]

Options:
  -s, -server <String>      Full URI of read/write endpoint of Triple Store. (Required)
  -p, -scripts <String>     Root path of migration query scripts. (Required)
  -v, -verbose <Boolean>    Set output to verbose messages. (Default: False)
```

Scripts can follow any naming convention that makes sense, 

## Example

Here's an example of how to invoke it with typical settings

```shell
$ Sparql.Migrator.exe -s "http://localhost:8889/blazegraph/namespace/kb/sparql" \
	-p "/home/andrew/dev/Sparql.Migrator/test-data/migrations"
```

There is a simple Docker Compose file provided that will allow you to test out your migrations.