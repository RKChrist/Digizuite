#!/usr/bin/env python
import pika
import io, os
import asyncio
from image_resizer import ImageResizer

import pprint

RABBIT_MQ_HOST = os.environ.get('RabbitMQServerHost', 'rabbitmq')
RABBIT_MQ_PORT = os.environ.get('RabbitMQServerPort', '5672')
RETRY_DELAY = 1

class Main:
    
    def __init__(self, resizer) -> None:
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
                print(f'channel created')
                return
            except Exception as e:
                print("failed to get channel")
                print(e)
                await self.delay()

    async def bind_channel(self):
        while True:
            try:
                self.channel.queue_bind(exchange=self.exchange_name, queue=self.queue_name)
                return
            except Exception as e:
                print('rabbit mq channel not ready yet')
                print(e)
                await self.delay()


    async def run(self):
        await self.create_connection()
        await self.create_channel()
        await self.bind_channel()
        
        self.channel = self.create_channel()
        self.channel.basic_consume(
            queue=self.queue_name, on_message_callback=self.callback, auto_ack=True)
        self.channel.start_consuming()
        print(' [*] Waiting for logs. To exit press CTRL+C')

    def callback(self, ch, method, properties, body):
        headers = properties.headers
        print('headers:\n', headers)
        resized_image = self.resizer.resize_from_bytes(body, 128, 128)
        pprint.pprint(properties)


if __name__ == '__main__':
    resizer = ImageResizer()
    app = Main(resizer=resizer)
    asyncio.run(app.run())