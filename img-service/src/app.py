#!/usr/bin/env python
import pika
import io
from image_resizer import ImageResizer

resizer = ImageResizer()
connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost', port=6000))
channel = connection.channel()

queue_name = 'q_images'
# channel.exchange_declare(exchange='e_files', exchange_type='headers')

# result = channel.queue_declare(queue='', exclusive=True)
# queue_name = result.method.queue

channel.queue_bind(exchange='e_files', queue='q_images')

print(' [*] Waiting for logs. To exit press CTRL+C')

def callback(ch, method, properties, body):
    resized_image = resizer.resize_from_bytes(body, 128, 128)
    
    print(type(body))
    print(f" [x] {properties}")

channel.basic_consume(
    queue=queue_name, on_message_callback=callback, auto_ack=True)

channel.start_consuming()