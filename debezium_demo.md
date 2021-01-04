# Debezium K8 Demo

This document will show the steps needed to deploy the following.

Create a Local Kubernetes Cluster

Configure the following services on the cluster

- Kafka Cluster
- Debezium Connect 
- Postgres DB

## Create Desktop Cluster 

Use [Kind](https://kind.sigs.k8s.io/)

Create a kind-3node.yaml file to define cluster
``` yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
  - role: worker
```

Build Cluster
``` bash
kind create cluster --name k8cluster --config ./kind-3nodes.yaml
```

To start over (delete cluster)
``` bash
kind delete cluster --name k8cluster
```


## Kafka Setup

Use Bitnami Helm Chart

``` bash
helm repo add bitnami https://charts.bitnami.com/bitnami
```
Get values.yaml from [here](https://raw.githubusercontent.com/bitnami/charts/master/bitnami/kafka/values.yaml), save locally.

Update values.yaml as needed. i.e.

``` yaml
replicaCount: 3
```

Dry Run Values to make sure no mistakes where made
```
helm install my-kafka bitnami/kafka --dry-run -f values.yaml
```

Deploy Kafka
```
helm install my-kafka bitnami/kafka -f values.yaml
```

You should get the following output

``` bash
NAME: my-kafka
LAST DEPLOYED: Tue Dec 29 10:49:40 2020
NAMESPACE: default
STATUS: deployed
REVISION: 1
TEST SUITE: None
NOTES:
** Please be patient while the chart is being deployed **

Kafka can be accessed by consumers via port 9092 on the following DNS name from within your cluster:

    my-kafka.default.svc.cluster.local

Each Kafka broker can be accessed by producers via port 9092 on the following DNS name(s) from within your cluster:

    my-kafka-0.my-kafka-headless.default.svc.cluster.local:9092

To create a pod that you can use as a Kafka client run the following commands:

    kubectl run my-kafka-client --restart='Never' --image docker.io/bitnami/kafka:2.7.0-debian-10-r1 --namespace default --command -- sleep infinity
    kubectl exec --tty -i my-kafka-client --namespace default -- bash

    PRODUCER:
        kafka-console-producer.sh \

            --broker-list my-kafka-0.my-kafka-headless.default.svc.cluster.local:9092 \
            --topic test

    CONSUMER:
        kafka-console-consumer.sh \

            --bootstrap-server my-kafka.default.svc.cluster.local:9092 \
            --topic test \
            --from-beginning
```

## Debeziuim Connect Depoyment 

Create file: debezium-connect-deploy.yaml

Note: 
- BOOTSTRAP_SERVERS need to match kafka nodes
- GROUP_ID is used for [Distributed Mode](https://docs.confluent.io/platform/current/connect/userguide.html#distributed-mode)

``` yaml
          env:
            - name: BOOTSTRAP_SERVERS
              value: my-kafka.default.svc.cluster.local:9092
```

``` yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: debezium-connect-deploy
  labels: 
    app: debezium-connect
spec:
  replicas: 3
  selector:
    matchLabels:
      app: debezium-connect
  template:
    metadata:
      labels: 
        app: debezium-connect
    spec:
      containers:
        - name: debezium-connect
          image: debezium/connect:latest
          imagePullPolicy: IfNotPresent
          resources:
            requests:
              memory: "64Mi"
              cpu: "250m"
            limits:
              memory: "128Mi"
              cpu: "500m"
          ports:
            - containerPort: 8083
          env:
            - name: BOOTSTRAP_SERVERS
              value: my-kafka.default.svc.cluster.local:9092
            - name: GROUP_ID
              value: "1"
            - name: OFFSET_STORAGE_TOPIC
              value: connect-offsets
            - name: CONFIG_STORAGE_TOPIC
              value: connect-configs
            - name: STATUS_STORAGE_TOPIC
              value: connect-status
---
apiVersion: v1
kind: Service
metadata:
  name: debezium-connect-service
  labels:
    app: debezium-connect-service
spec:
  type: NodePort
  ports:
    - name: debezium-connect
      protocol: TCP
      port: 8083
      nodePort: 30500
  selector:
      app: debezium-connect
```

Deploy Debezium Connectd
``` bash
kubectl apply  -f debezium-connect-deploy.yaml
```


# Create Postgres Database

Using [Bitnami](https://bitnami.com/stack/postgresql/helm) for testing

Create File: extended.conf
```
  # extended.conf
  wal_level = logical
  max_wal_senders = 1
  max_replication_slots = 1
```

Create Config Map from extended.conf file
``` bash
kubectl create configmap  --from-file=extended.conf postgresql-config
```

Deploy Postgress Helm Chart

Note: password
```
helm install postgres bitnami/postgresql --set extendedConfConfigMap=postgresql-config --set service.type=NodePort --set service.nodePort=30600 --set postgresqlPassword=passw0rd
```

You should get the following output

```
NAME: postgres
LAST DEPLOYED: Wed Dec 30 09:29:56 2020
NAMESPACE: default
STATUS: deployed
REVISION: 1
TEST SUITE: None
NOTES:
** Please be patient while the chart is being deployed **

PostgreSQL can be accessed via port 5432 on the following DNS name from within your cluster:

    postgres-postgresql.default.svc.cluster.local - Read/Write connection

To get the password for "postgres" run:

    export POSTGRES_PASSWORD=$(kubectl get secret --namespace default postgres-postgresql -o jsonpath="{.data.postgresql-password}" | base64 --decode)

To connect to your database run the following command:

    kubectl run postgres-postgresql-client --rm --tty -i --restart='Never' --namespace default --image docker.io/bitnami/postgresql:11.10.0-debian-10-r24 --env="PGPASSWORD=$POSTGRES_PASSWORD" --command -- psql --host postgres-postgresql -U postgres -d postgres -p 5432



To connect to your database from outside the cluster execute the following commands:

    export NODE_IP=$(kubectl get nodes --namespace default -o jsonpath="{.items[0].status.addresses[0].address}")
    export NODE_PORT=$(kubectl get --namespace default -o jsonpath="{.spec.ports[0].nodePort}" services postgres-postgresql)
    PGPASSWORD="$POSTGRES_PASSWORD" psql --host $NODE_IP --port $NODE_PORT -U postgres -d postgres
```

## Create Table / Data

Shell into Postgress
``` bash
kubectl exec -it postgres-postgresql-0  -- /bin/sh
```
Login to Postgres
``` bash
psql --user postgres
```

Create a table named `containers`
``` bash
CREATE TABLE containers(containerid VARCHAR(30) NOT NULL,type VARCHAR(20),status VARCHAR(20),brand VARCHAR(50),capacity DECIMAL,CREATIONDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP,UPDATEDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP,PRIMARY KEY (containerid));
```
Insert data into the table.
``` bash
  INSERT INTO containers (containerid, type, status, brand, capacity) VALUES ('C01','Reefer','Operational','containerbrand',20), ('C02','Dry','Operational','containerbrand',20), ('C03','Dry','Operational','containerbrand',40), ('C04','FlatRack','Operational','containerbrand',40), ('C05','OpenTop','Operational','containerbrand',40), ('C06','OpenSide','Operational','containerbrand',40), ('C07','Tunnel','Operational','containerbrand',40), ('C08','Tank','Operational','containerbrand',40), ('C09','Thermal','Operational','containerbrand',20);
```
## Configure the Debezium PostgreSQL connector
Verify Postgres Plugin is install on connector
``` bash
kubectl exec -i my-kafka-0 -- curl -X GET http://debezium-connect-service:8083/connector-plugins
```

Configure Debezium Connector to watch `public.containers` table

Additional Settings from [here](https://debezium.io/blog/2020/02/25/lessons-learned-running-debezium-with-postgresql-on-rds/)

Connector Setting [here](https://debezium.io/documentation/reference/1.3/connectors/postgresql.html#postgresql-connector-properties)

TODO: Better undertand, SLOT.NAME .. should be debezium

``` bash 
kubectl exec -i my-kafka-0 -- curl -X POST   http://debezium-connect-service:8083/connectors   -H 'Content-Type: application/json'   -d '{
    "name": "containers-connector",
    "config": {
            "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
            "plugin.name": "pgoutput",
            "database.hostname": "postgres-postgresql",
            "database.port": "5432",
            "database.user": "postgres",
            "database.password": "passw0rd",
            "database.dbname": "postgres",
            "database.server.name": "postgres",
            "table.whitelist": "public.containers",
            "decimal.handling.mode": "string",
            "slot.name": "debezium"
      }
}'
```

To Delete / Remove Connector
``` bash
kubectl exec -i my-kafka-0 -- curl -X DELETE http://debezium-connect-service:8083/connectors/containers-connector
```

To Update Connector

``` bash 
kubectl exec -i my-kafka-0 -- curl -X PUT   http://debezium-connect-service:8083/connectors/containers-connector/config  -H 'Content-Type: application/json'   -d '{
            "connector.class": "io.debezium.connector.postgresql.PostgresConnector",
            "plugin.name": "pgoutput",
            "database.hostname": "postgres-postgresql",
            "database.port": "5432",
            "database.user": "postgres",
            "database.password": "passw0rd",
            "database.dbname": "postgres",
            "database.server.name": "postgres",
            "table.whitelist": "public.containers",
            "decimal.handling.mode": "string",
            "slot.name": "debezium"
      }'

```

Get Connector Status 
``` bash
kubectl exec -i my-kafka-0 -- curl -X GET   http://debezium-connect-service:8083/connectors?expand=status
```

## View Topic Output

Create Kafka Client
``` bash
kubectl run my-kafka-client --restart='Never' --image docker.io/bitnami/kafka:2.7.0-debian-10-r1 --command -- sleep infinity
```

Bash into kafka client 
``` bash
kubectl exec --tty -i my-kafka-client  -- bash
```

List Topics in Kakfa 
``` bash
kafka-topics.sh --bootstrap-server my-kafka.default.svc.cluster.local:9092 --list
```
Expect Output 
``` bash
__consumer_offsets
connect-configs
connect-offsets
connect-status
postgres.public.containers
```

View/Consume Message in Kakfa Topic

Note: To get `before` values `REPLICA IDENTITY` needs to be [set](https://debezium.io/documentation/reference/1.3/connectors/postgresql.html#postgresql-replica-identity) to `FULL`.  Default is `NOTHING`

``` bash
kafka-console-consumer.sh --topic postgres.public.containers --from-beginning --bootstrap-server my-kafka.default.svc.cluster.local:9092
```

# Links

The links below are what I used to build this document.

## Documentation 

- https://debezium.io/documentation/reference/1.3/tutorial.html
- https://debezium.io/documentation/reference/1.3/connectors/postgresql.html
- https://debezium.io/documentation/reference/postgres-plugins.html
- https://docs.confluent.io/platform/current/connect/userguide.html#standalone-vs-distributed-mode
- https://raw.githubusercontent.com/bitnami/charts/master/bitnami/kafka/values.yaml
- https://docs.confluent.io/platform/current/connect/references/restapi.html

## Tools
- https://v3.helm.sh/docs/intro/quickstart/
- https://strimzi.io/docs/operators/latest/quickstart.html
- https://kubernetes.io/docs/reference/kubectl/cheatsheet/
- https://kind.sigs.k8s.io/
- https://hub.docker.com/r/debezium/connect
- https://kubernetes.io/blog/2020/05/21/wsl-docker-kubernetes-on-the-windows-desktop/
- https://github.com/bitnami/charts/tree/master/bitnami/postgresql
- https://bitnami.com/stack/postgresql/helm


## Blogs
- https://wecode.wepay.com/posts/streaming-databases-in-realtime-with-mysql-debezium-kafka
- https://medium.com/blablacar/streaming-data-out-of-the-monolith-building-a-highly-reliable-cdc-stack-d71599131acb
- https://www.youtube.com/watch?v=aO2pv8W6oZU
- https://debezium.io/blog/2020/02/25/lessons-learned-running-debezium-with-postgresql-on-rds/
- https://debezium.io/blog/2019/02/19/reliable-microservices-data-exchange-with-the-outbox-pattern/
- https://debezium.io/blog/2020/09/15/debezium-auto-create-topics/
- https://debezium.io/blog/2020/02/10/event-sourcing-vs-cdc/
- https://www.confluent.io/blog/apache-kafka-kubernetes-could-you-should-you/
- https://www.magalix.com/blog/kafka-on-kubernetes-and-deploying-best-practice
- https://www.redhat.com/en/topics/integration/why-run-apache-kafka-on-kubernetes
- https://medium.com/@sincysebastian/setup-kafka-with-debezium-using-strimzi-in-kubernetes-efd494642585
- https://medium.com/@tienbm90/change-data-capture-install-debezium-on-k8s-8a98a55a1406
- https://medium.com/swlh/apache-kafka-with-kubernetes-provision-and-performance-81c61d26211c
- https://ibm-cloud-architecture.github.io/refarch-container-inventory/debezium-postgresql/
- https://developers.redhat.com/blog/2020/05/08/change-data-capture-with-debezium-a-simple-how-to-part-1/
- https://docs.bitnami.com/tutorials/build-messaging-cluster-apache-kafka-mongodb-kubernetes/
- https://docs.bitnami.com/tutorials/deploy-scalable-kafka-zookeeper-cluster-kubernetes/
- https://debezium.io/blog/2016/05/31/Debezium-on-Kubernetes/
- https://manhtai.github.io/posts/debezium-kafka-connect-kubernetes/
- https://www.youtube.com/watch?v=b-3qN_tlYR4&t=981


## Kafka
- https://www.confluent.io/blog/designing-the-net-api-for-apache-kafka/