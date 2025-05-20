using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace SmartSync.Infraestructure.Messaging.Publisher;

public class Publisher
{
    public static void SendMessage(string postagemId)
    {
        var factory = new ConnectionFactory { HostName = "amqps://jzbrpolr:sEJrOHF_O46SJenrAhxBvKbL-IZwvNPP@jackal.rmq.cloudamqp.com/jzbrpolr" };

        using var connection = factory.CreateConnection();
        using var model = connection.CreateModel();

        var body = Encoding.UTF8.GetBytes(postagemId);

        string exchangeName = "meu_exchange_fanout";
        model.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Fanout
        );

        var properties = model.CreateBasicProperties();
        properties.Persistent = true;

        model.BasicPublish(
            exchange: exchangeName,
            routingKey: "",
            body: body,
            basicProperties: properties
        );

        Console.WriteLine("Mensagem enviada!");
    }
}