FROM rustlang/rust:nightly

WORKDIR /usr/src/app
COPY . .

RUN cargo install --path .

CMD ["pdf-service"]
