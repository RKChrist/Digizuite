use std::env;
use std::time::Duration;

use lapin::{
    message::DeliveryResult,
    options::{BasicAckOptions, BasicConsumeOptions},
    types::FieldTable,
    Connection, ConnectionProperties, Channel, Consumer,
};

const QUEUE_NAME: &str = "q_pdf";
const CONSUMER_TAG: &str = "pdf-service-tag";

async fn connect_rabbitmq(uri: &str, connection_properties: ConnectionProperties) -> Connection {
    let mut connection = Connection::connect(uri, connection_properties.clone()).await;

    while connection.is_err() {
        println!(
            "--> Failed to connect to rabbitmq: {}",
            &connection.unwrap_err()
        );
        println!("--> Attempting to reconnect in 3 seconds...");
        std::thread::sleep(Duration::from_millis(3000));
        connection = Connection::connect(uri, connection_properties.clone()).await;
    }
    println!("--> Connected to rabbitmq!");
    // There might be a way to register a callback to handle dropped connections here
    let connection = connection.unwrap();
    connection
}

async fn consume_pdf_queue(channel: &Channel) -> Consumer {
    let mut consumer = channel.basic_consume(QUEUE_NAME, CONSUMER_TAG, BasicConsumeOptions::default(), FieldTable::default()).await;

    while consumer.is_err() {
        println!("--> Failed to consume queue: {}", &consumer.unwrap_err());
        println!("--> Attempting to re-consume queue in 3 seconds...");
        std::thread::sleep(Duration::from_millis(3000));
        consumer = channel.basic_consume(QUEUE_NAME, CONSUMER_TAG, BasicConsumeOptions::default(), FieldTable::default()).await;
    }
    
    let consumer = consumer.unwrap();
    consumer
}

#[tokio::main]
async fn main() {
    // Default URI will connect to default virtual host `/`
    // source https://docs.rs/lapin/latest/lapin/struct.Connection.html
    let uri: String = env::var("RABBITMQ_URI")
        .unwrap_or_else(|_| "amqp://guest:guest@localhost:5673/%2F".to_string());

    let options = ConnectionProperties::default()
        .with_connection_name("pdf-service-connection".to_string().into())
        .with_executor(tokio_executor_trait::Tokio::current())
        .with_reactor(tokio_reactor_trait::Tokio);

    let connection = connect_rabbitmq(&uri, options).await;

    let channel = connection.create_channel().await.unwrap();

    let consumer = consume_pdf_queue(&channel).await;

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
            }
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
