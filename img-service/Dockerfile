from alpine

ENV PYTHONUNBUFFERED=1
ENV RABBIT_MQ_HOST=rabbitmq
ENV RABBIT_MQ_HOST=5672
RUN apk add --update --no-cache python3 && ln -sf python3 /usr/bin/python
RUN python3 -m ensurepip
RUN pip3 install --no-cache --upgrade pip setuptools
WORKDIR /usr/src/app
COPY ./src /usr/src/app
RUN pip3 install -r requirements.txt

CMD [ "python","-u", "app.py" ]

