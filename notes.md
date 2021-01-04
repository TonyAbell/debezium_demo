kubectl create namespace kafka

wget https://github.com/strimzi/strimzi-kafka-operator/releases/download/0.20.1/strimzi-0.20.1.zip

https://medium.com/@tienbm90/change-data-capture-install-debezium-on-k8s-8a98a55a1406

curl -L https://github.com/strimzi/strimzi-kafka-operator/releases/download/0.20.1/strimzi-cluster-operator-0.20.1.yaml \
  | sed 's/namespace: .*/namespace: kafka/' \
  | kubectl apply -f - -n kafka

kubectl -n kafka \
    apply -f https://raw.githubusercontent.com/strimzi/strimzi-kafka-operator/0.20.1/examples/kafka/kafka-persistent-single.yaml \
  && kubectl wait kafka/my-cluster --for=condition=Ready --timeout=300s -n kafka


curl https://repo1.maven.org/maven2/io/debezium/debezium-connector-postgres/1.3.1.Final/debezium-connector-postgres-1.3.1.Final-plugin.tar.gz \
| tar xvz

https://repo1.maven.org/maven2/io/debezium/debezium-connector-postgres/1.3.1.Final/debezium-connector-postgres-1.3.1.Final-plugin.tar.gz


https://search.maven.org/remotecontent?filepath=org/mongodb/kafka/mongo-kafka-connect/1.2.0/mongo-kafka-connect-1.2.0-all.jar


https://helm.sh/docs/intro/install/
curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3
sudo chmod 700 get_helm.sh
helm repo add stable https://charts.helm.sh/stable
helm repo add incubator https://charts.helm.sh/incubator

helm repo add bitnami https://charts.bitnami.com/bitnami
helm install my-kafka bitnami/kafka
helm install my-kafka bitnami/kafka --dry-run -f values.yaml




https://ibm-cloud-architecture.github.io/refarch-container-inventory/debezium-postgresql/
https://medium.com/@tienbm90/change-data-capture-install-debezium-on-k8s-8a98a55a1406


https://strimzi.io/docs/operators/latest/quickstart.html
https://medium.com/@yimin.zheng/kafka-on-kubernetes-strimzi-vs-confluent-operators-df5ea81df5c8
https://www.redhat.com/en/topics/integration/why-run-apache-kafka-on-kubernetes



Docker WSL2
https://kubernetes.io/blog/2020/05/21/wsl-docker-kubernetes-on-the-windows-desktop/
`
kubectl -n kubernetes-dashboard describe secret $(kubectl -n kubernetes-dashboard get secret | grep admin-user | awk '{print $1}')
`

https://kubernetes.io/docs/reference/kubectl/cheatsheet/


https://strimzi.io/docs/operators/latest/quickstart.html
```
kubectl create ns kafka
kubectl create ns my-kafka-project
cd strimzi-0.20.1/
sed -i 's/namespace: .*/namespace: kafka/' install/cluster-operator/*RoleBinding*.yaml
code install/cluster-operator/060-Deployment-strimzi-cluster-operator.yaml
kubectl apply -f install/cluster-operator/ -n kafka


kubectl apply -f install/cluster-operator/020-RoleBinding-strimzi-cluster-operator.yaml -n my-kafka-project
kubectl apply -f install/cluster-operator/032-RoleBinding-strimzi-cluster-operator-topic-operator-delegation.yaml -n my-kafka-project
kubectl apply -f install/cluster-operator/031-RoleBinding-strimzi-cluster-operator-entity-operator-delegation.yaml -n my-kafka-project
kubectl create -n my-kafka-project -f ../my-cluster.yaml
```

Kind
https://kind.sigs.k8s.io/
https://kubernetes.io/blog/2020/05/21/wsl-docker-kubernetes-on-the-windows-desktop/


Create 
```
kind create cluster --name k8cluster --config ./kind-3nodes.yaml
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.1.0/aio/deploy/recommended.yaml

kubectl apply -f - <<EOF
apiVersion: v1
kind: ServiceAccount
metadata:
  name: admin-user
  namespace: kubernetes-dashboard
EOF


kubectl apply -f - <<EOF
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: admin-user
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: cluster-admin
subjects:
- kind: ServiceAccount
  name: admin-user
  namespace: kubernetes-dashboard
EOF

kubectl -n kubernetes-dashboard describe secret $(kubectl -n kubernetes-dashboard get secret | grep admin-user | awk '{print $1}')
```

Remove 
```
kind delete cluster --name k8cluster
```


Helm Bitnami Kakfa 
```

helm repo add bitnami https://charts.bitnami.com/bitnami
helm install my-kafka bitnami/kafka

```
https://docs.bitnami.com/tutorials/build-messaging-cluster-apache-kafka-mongodb-kubernetes/

```
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
https://debezium.io/documentation/reference/1.3/tutorial.html

https://medium.com/@sincysebastian/setup-kafka-with-debezium-using-strimzi-in-kubernetes-efd494642585
https://medium.com/@tienbm90/change-data-capture-install-debezium-on-k8s-8a98a55a1406


kubectl exec -i my-kafka-0 -- bin/kafka-console-producer.sh --broker-list my-kafka-0:9093 --topic my-topic
kubectl exec -i my-kafka-0 -- bin/kafka-console-consumer.sh --bootstrap-server my-kafka-0:9093 --topic my-topic --from-beginning



https://search.maven.org/remotecontent?filepath=org/mongodb/kafka/mongo-kafka-connect/1.2.0/mongo-kafka-connect-1.2.0-all.jar



https://bitnami.com/stack/postgresql/helm
https://github.com/bitnami/charts/tree/master/bitnami/postgresql

kubectl create configmap  --from-file=extended.conf postgresql-config
helm install postgres bitnami/postgresql --set extendedConfConfigMap=postgresql-config --set service.type=NodePort --set service.nodePort=30600 --set postgresqlPassword=passw0rd 

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

kubectl exec -it postgres-postgresql-0  -- /bin/sh
psql --user postgres
  CREATE TABLE containers(containerid VARCHAR(30) NOT NULL,type VARCHAR(20),status VARCHAR(20),brand VARCHAR(50),capacity DECIMAL,CREATIONDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP,UPDATEDATE TIMESTAMP DEFAULT CURRENT_TIMESTAMP,PRIMARY KEY (containerid));

    INSERT INTO containers (containerid, type, status, brand, capacity) VALUES ('C01','Reefer','Operational','containerbrand',20), ('C02','Dry','Operational','containerbrand',20), ('C03','Dry','Operational','containerbrand',40), ('C04','FlatRack','Operational','containerbrand',40), ('C05','OpenTop','Operational','containerbrand',40), ('C06','OpenSide','Operational','containerbrand',40), ('C07','Tunnel','Operational','containerbrand',40), ('C08','Tank','Operational','containerbrand',40), ('C09','Thermal','Operational','containerbrand',20);




kubectl exec -i my-kafka-0 -- curl -X GET http://debezium-connect-service:8083/connector-plugins

kubectl exec -i my-kafka-0 -- curl -X POST \
  http://debezium-connect-service:8083/connectors \
  -H 'Content-Type: application/json' \
  -d '{
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
            "table.whitelist": "public.containers"
      }
}'


```
NAME: my-kafka
LAST DEPLOYED: Wed Dec 30 09:11:29 2020
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
    my-kafka-1.my-kafka-headless.default.svc.cluster.local:9092
    my-kafka-2.my-kafka-headless.default.svc.cluster.local:9092

To create a pod that you can use as a Kafka client run the following commands:

    kubectl run my-kafka-client --restart='Never' --image docker.io/bitnami/kafka:2.7.0-debian-10-r1 --namespace default --command -- sleep infinity
    kubectl exec --tty -i my-kafka-client --namespace default -- bash

    PRODUCER:
        kafka-console-producer.sh \

            --broker-list my-kafka-0.my-kafka-headless.default.svc.cluster.local:9092,my-kafka-1.my-kafka-headless.default.svc.cluster.local:9092,my-kafka-2.my-kafka-headless.default.svc.cluster.local:9092 \
            --topic test

    CONSUMER:
        kafka-console-consumer.sh \

            --bootstrap-server my-kafka.default.svc.cluster.local:9092 \
            --topic test \
            --from-beginning
```

kafka-topics.sh --bootstrap-server my-kafka.default.svc.cluster.local:9092 --list
kafka-console-consumer.sh --topic postgres.public.containers --from-beginning --bootstrap-server my-kafka.default.svc.cluster.local:9092