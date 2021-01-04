# Web Demo

This assumes you have completed the steps in `debezium_demo.md`

This is a walk through for creating a database with a CRUD Web App, where all changes are Synced to another Database with a Web App to view.

This demo sets up two databases.
- Source/Primary where changes are made.
- Proxy where changes are synced to 

This demo sets up three applications 
- Consumer: worker which monitors for changes and updates the Proxy database
- Source/Primary: Basic CRUD Web App for Source database
- Proxy: a basic Read Only Web App for Proxy database

How to configure Debezium Connect to watch for changes in Source Database.

## Source DB

Should have been created from `debezium-demo.md`

## Create Proxy DB To Store Synced Data

Deploy Postgress Helm Chart

Note: password
```
helm install proxy-postgres bitnami/postgresql  --set postgresqlPassword=passw0rd
```

# Demo Apps

Settings are stored in `ENV VARS`, which are configured in the *-deploy.yaml

When testing localy, make sure you forward ports and set `ENV VARS`, when running apps.

Note: in testing the DB was not always created by EF.  To force creation of DB use `ef database update command`.  Make sure you forward the port of the db you want to init.

``` bash
cd dal
dotnet ef database update
```


## Consumer 
Reads CDC changes from the Kafka Topic and updates the Proxy DB

## Source
Is a Web App over a DB which allows CRUD operations on a few tables

## Proxy
Is a Read Only Web App over a DB which recieved changes from `Source`

Withing the `demo` folder, build and deploy the apps.

Build/Deploy consumer Processor
```bash 
docker build -t consumer:dev -f consumer/Dockerfile . && kind load docker-image consumer:dev --name k8cluster && kubectl apply -f ./consumer/consumer-deploy.yaml
```

Build/Deploy Source Web App
```bash 
docker build -t source:dev -f source/Dockerfile . && kind load docker-image source:dev --name k8cluster && kubectl apply -f ./source/source-deploy.yaml
```

Build/Deploy Proxy Web App
```bash 
docker build -t proxy:dev -f proxy/Dockerfile . && kind load docker-image proxy:dev --name k8cluster && kubectl apply -f ./proxy/proxy-deploy.yaml
```

# Update Connect

There can only be one connection to a posgres db on the Debezium Connect 

TODO: understand why there can only be one connector to a postgres db.

Delete connector from previous demo.
```bash
kubectl exec -i my-kafka-0 -- curl -X DELETE http://debezium-connect-service:8083/connectors/containers-connector
```

Create connector to watch new tables.
``` bash 
kubectl exec -i my-kafka-0 -- curl -X POST   http://debezium-connect-service:8083/connectors   -H 'Content-Type: application/json'   -d '{
    "name": "student-connector",
    "config": {
            "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
            "plugin.name": "pgoutput",
            "database.hostname": "postgres-postgresql",
            "database.port": "5432",
            "database.user": "postgres",
            "database.password": "passw0rd",
            "database.dbname": "postgres",
            "database.server.name": "postgres",
            "table.whitelist": "public.Student,public.Course",
            "decimal.handling.mode": "string",
            "slot.name": "debezium"
      }
}'
```

### Remove Connector

If there are issues / error, remove the connector created.

``` bash
kubectl exec -i my-kafka-0 -- curl -X DELETE http://debezium-connect-service:8083/connectors/student-connector
```


## Forward `Source` Web App

To Access the Source Web App use port 5000

Use Source Web App to CRUD data.

```bash
kubectl port-forward services/source-service 5000:5000 
```

## Forward `Proxy` Web App

To Access the PRoxy Web App user port 5001

Use Prxy Web App to view Changes from Source

``` bash
kubectl port-forward services/proxy-service 5001:5000 -n default
```

## Forward Postgres
To Access Postgres DB forward its port.

You can edit data in Primary (Source) directly and call changes to be moved to Proxy

### Proxy 
``` bash
kubectl port-forward services/proxy-postgres-postgresql 5432:5432
```

### Source / Primary
``` bash
kubectl port-forward services/postgres-postgresql 5432:5432
```

# Known Issues

Debezium converts Postgres Timestamps to MicroSeconds.  When using Dotnet to convert, there seems to be a loss in resolution for MicroSeconds.