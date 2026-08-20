## Kafka Commands for Topic Management

### List all topics
```bash
docker exec kafka kafka-topics --list --bootstrap-server localhost:9092
```

### Create a topic
```bash
docker exec kafka kafka-topics --create --topic producer-sync-item --partitions 1 --replication-factor 1 --bootstrap-server localhost:9092
```

### Describe a topic
```bash
docker exec kafka kafka-topics --describe --topic producer-sync-item --bootstrap-server localhost:9092
```

### Delete a topic
```bash
docker exec kafka kafka-topics --delete --topic producer-sync-item --bootstrap-server localhost:9092
```

### Produce a test message
```bash
docker exec -it kafka kafka-console-producer --topic producer-sync-item --bootstrap-server localhost:9092
```
Then type your JSON message and press Ctrl+D to send

### Consume messages from a topic
```bash
docker exec -it kafka kafka-console-consumer --topic producer-sync-item --from-beginning --bootstrap-server localhost:9092
```

### Check consumer groups
```bash
docker exec kafka kafka-consumer-groups --list --bootstrap-server localhost:9092
```

### Check consumer group details
```bash
docker exec kafka kafka-consumer-groups --describe --group ewp-sf-item-group --bootstrap-server localhost:9092
# Repeat for other entity types
