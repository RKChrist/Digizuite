use std::time::Duration;

use lapin::{
    message::DeliveryResult,
    options::{self, BasicConsumeOptions, BasicAckOptions},
    types::FieldTable,
    Connection, ConnectionProperties,
};

#[tokio::main]
async fn main() {
    let uri = "amqp://guest:guest@localhost:5673/%2F";
    let options = ConnectionProperties::default()
        .with_executor(tokio_executor_trait::Tokio::current())
        .with_reactor(tokio_reactor_trait::Tokio);

    let connection = Connection::connect(uri, options).await.unwrap();
    let channel = connection.create_channel().await.unwrap();

    // let mut arguments = FieldTable::default();

    // arguments.insert("x-filetype".into(), AMQPValue::LongString("pdf".into()));

    // let _exchange = channel.exchange_declare("e_files", ExchangeKind::Headers, options::ExchangeDeclareOptions::default(), arguments).await;

    let _queue = channel
        .queue_declare(
            "q_pdf",
            options::QueueDeclareOptions::default(),
            FieldTable::default(),
        )
        .await
        .unwrap();

    println!("--> Declared queue");

    let consumer = channel
        .basic_consume(
            "q_pdf",
            "pdf-service-tag",
            BasicConsumeOptions::default(),
            FieldTable::default(),
        )
        .await
        .unwrap();

    println!("--> Waiting for messages...");

    consumer.set_delegate(move |delivery: DeliveryResult| async move {
        let delivery = match delivery {
            // Carries the delivery alongside its channel
            Ok(Some(delivery)) => delivery,
            // The consumer got canceled
            Ok(None) => return,
            // Carries the error and is always followed by Ok(None)
            Err(error) => {
                dbg!("Failed to consume queue message {}", error);
                return;
            },
        };

        // Simulate processing...
        println!("--> Message received! Processing...");
        std::thread::sleep(Duration::from_millis(2000));
        println!("--> Process complete! acknowledging...");

        // TODO: Expose an endpoint to download the processed file

        delivery
            .ack(BasicAckOptions::default())
            .await
            .expect("Failed to ack message");
    });

    std::future::pending::<()>().await;
}
