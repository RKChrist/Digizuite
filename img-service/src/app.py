#!/usr/bin/env python
import pika
import io, os
from image_resizer import ImageResizer

import pprint

RABBIT_MQ_HOST = os.environ.get('RabbitMQServerHost', 'rabbitmq')
RABBIT_MQ_PORT = os.environ.get('RabbitMQServerPort', '5672')

resizer = ImageResizer()

print('trying to connect to {RABBIT_MQ_HOST}:{RABBIT_MQ_PORT}')

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host=RABBIT_MQ_HOST, port=RABBIT_MQ_PORT))
channel = connection.channel()

queue_name = 'q_images'
# channel.exchange_declare(exchange='e_files', exchange_type='headers')

# result = channel.queue_declare(queue='', exclusive=True)
# queue_name = result.method.queue

channel.queue_bind(exchange='e_files', queue='q_images')

print(' [*] Waiting for logs. To exit press CTRL+C')

def callback(ch, method, properties, body):
    headers = properties.headers
    print('headers:\n', headers)
    resized_image = resizer.resize_from_bytes(body, 128, 128)
    pprint.pprint(properties)
channel.basic_consume(
    queue=queue_name, on_message_callback=callback, auto_ack=True)

channel.start_consuming()

## 332a885e79c5   rabbitmq:3.12.3-management-alpine "docker-entrypoint.s…"
# 0.0.0.0:6000->5672/tcp
# 0.0.0.0:8080->15672/tcp
# some-rabbit
