#!/usr/bin/env python
import pika
import io, os
import asyncio
from image_resizer import ImageResizer

import pprint

RABBIT_MQ_HOST = os.environ.get('RabbitMQServerHost', 'rabbitmq')
RABBIT_MQ_PORT = os.environ.get('RabbitMQServerPort', '5672')
RETRY_DELAY = 5 # seconds between attempts

class Main:
    
    def __init__(self, resizer) -> None:
        self.resizer = resizer
        self.channel = None
        self.connection = None
        self.queue_name = 'q_images'
        self.exchange_name = 'e_files'

    async def delay(self):
        return await asyncio.sleep(RETRY_DELAY)

    async def create_connection(self):
        print(f'trying to connect to {RABBIT_MQ_HOST}:{RABBIT_MQ_PORT}')
        while True:
            try:
                self.connection = pika.BlockingConnection(
                    pika.ConnectionParameters(host=RABBIT_MQ_HOST, port=RABBIT_MQ_PORT))
                print(
                    f'connected to RabbitMQ @ {RABBIT_MQ_HOST}:{RABBIT_MQ_PORT}')
                return
            except Exception as e:
                print(f'connection to {RABBIT_MQ_HOST}:{RABBIT_MQ_PORT} FAILED')
                await self.delay()


    async def create_channel(self):
        while True:
            try:
                self.channel = self.connection.channel()
                self.channel.queue_bind(exchange=self.exchange_name, queue=self.queue_name)
                print(f'channel created')
                return
            except Exception as e:
                print('rabbit mq channel not ready yet')
                print(e)
                await self.delay()


    async def run(self):
        await self.create_connection()
        await self.create_channel()

        self.channel.basic_consume(
            queue=self.queue_name, on_message_callback=self.callback, auto_ack=True)
        self.channel.start_consuming()
        print(' [*] Waiting for logs. To exit press CTRL+C')

    def callback(self, ch, method, properties, body):
        try:
            print(properties)
            image_width = self.get_from_headers_or_default(properties, 'width', 128)
            image_height = self.get_from_headers_or_default(
                properties, 'height', 128)
            file_ext = self.get_from_headers_or_default(properties, 'extension', 128)
            resized_image = self.resizer.resize_from_bytes(body, image_height, image_width, file_ext)
            pprint.pprint(properties)
        except:
            print("Message processing failed")

    def get_from_headers_or_default(self, properties, key, default):
        return properties.headers[key] if properties.headers and properties.headers[key] else default


if __name__ == '__main__':
    resizer = ImageResizer()
    app = Main(resizer=resizer)
    asyncio.run(app.run())