FROM rustlang/rust:nightly

# Environment variable for setting rabbitmq connection uri
# ENV RABBITMQ_URI="amqp://guest:guest@localhost:5673/%2F"

WORKDIR /usr/src/app
COPY . .

RUN cargo install --path .

CMD ["pdf-service"]