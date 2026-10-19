using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using LabTrack.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabTrack.Infrastructure.Messaging;

/// <summary>Configuração da mensageria (seção "Messaging" do appsettings).</summary>
public class MessagingOptions
{
    public const string SectionName = "Messaging";

    /// <summary>"Logging" (padrão, não requer credenciais) ou "Sqs".</summary>
    public string Provider { get; set; } = "Logging";

    /// <summary>URL da fila SQS, ex.: https://sqs.us-east-1.amazonaws.com/123456789012/labtrack-out-of-spec</summary>
    public string? SqsQueueUrl { get; set; }
}

/// <summary>
/// Implementação padrão: apenas registra o evento no log.
/// Permite rodar o projeto localmente sem conta AWS.
/// </summary>
public class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(OutOfSpecDetectedEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation("[Evento] OutOfSpecDetected: {Event}", JsonSerializer.Serialize(@event));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Publica o evento em uma fila Amazon SQS, para ser consumido por outros serviços
/// (ex.: AWS Lambda que notifica a equipe de qualidade).
/// </summary>
public class SqsEventPublisher(
    IAmazonSQS sqs,
    IOptions<MessagingOptions> options,
    ILogger<SqsEventPublisher> logger) : IEventPublisher
{
    public async Task PublishAsync(OutOfSpecDetectedEvent @event, CancellationToken ct = default)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = options.Value.SqsQueueUrl,
            MessageBody = JsonSerializer.Serialize(@event),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["EventType"] = new() { DataType = "String", StringValue = nameof(OutOfSpecDetectedEvent) }
            }
        };

        try
        {
            var response = await sqs.SendMessageAsync(request, ct);
            logger.LogInformation("Evento enviado ao SQS (MessageId {MessageId})", response.MessageId);
        }
        catch (AmazonSQSException ex)
        {
            // Falha na mensageria não deve impedir o registro do resultado.
            logger.LogError(ex, "Falha ao publicar evento no SQS");
        }
    }
}
