using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.OpenApi;
using Shouldly;
using Xunit;

namespace OpenApiToCS.Tests;

public class WebhookTests
{
    private static OpenApiDocument LoadPaymentWebhooksSpec()
    {
        var json = File.ReadAllText("TestData/payment-webhooks-api.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Deserialize<OpenApiDocument>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize OpenAPI document");
    }

    [Fact]
    public void Should_ParseWebhooksFromSpec()
    {
        var document = LoadPaymentWebhooksSpec();
        
        document.Webhooks.ShouldNotBeNull();
        document.Webhooks.Count.ShouldBe(3);
        document.Webhooks.Keys.ShouldContain("paymentCompleted");
        document.Webhooks.Keys.ShouldContain("paymentFailed");
        document.Webhooks.Keys.ShouldContain("refundProcessed");
    }

    [Fact]
    public void Should_GenerateWebhookInterface()
    {
        var document = LoadPaymentWebhooksSpec();
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var generator = new WebhookGenerator(document, dataClasses);
        
        var webhooks = generator.GenerateWebhooks("PaymentApiV1");
        
        webhooks.ShouldNotBeNull();
        webhooks.ShouldContainKey("IPaymentAPIWebhookHandler");
        
        var interfaceCode = webhooks["IPaymentAPIWebhookHandler"];
        interfaceCode.ShouldContain("interface IPaymentAPIWebhookHandler");
        interfaceCode.ShouldContain("Task OnPaymentCompletedAsync(PaymentCompletedEvent payload);");
        interfaceCode.ShouldContain("Task OnPaymentFailedAsync(PaymentFailedEvent payload);");
        interfaceCode.ShouldContain("Task OnRefundProcessedAsync(RefundEvent payload);");
    }

    [Fact]
    public void Should_GenerateWebhookHandler()
    {
        var document = LoadPaymentWebhooksSpec();
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var generator = new WebhookGenerator(document, dataClasses);
        
        var webhooks = generator.GenerateWebhooks("PaymentApiV1");
        
        webhooks.ShouldNotBeNull();
        webhooks.ShouldContainKey("PaymentAPIWebhookHandler");
        
        var handlerCode = webhooks["PaymentAPIWebhookHandler"];
        handlerCode.ShouldContain("class PaymentAPIWebhookHandler : IPaymentAPIWebhookHandler");
        handlerCode.ShouldContain("public virtual Task OnPaymentCompletedAsync(PaymentCompletedEvent payload)");
        handlerCode.ShouldContain("public virtual Task OnPaymentFailedAsync(PaymentFailedEvent payload)");
        handlerCode.ShouldContain("public virtual Task OnRefundProcessedAsync(RefundEvent payload)");
    }

    [Fact]
    public void Should_IncludeSummariesInWebhooks()
    {
        var document = LoadPaymentWebhooksSpec();
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var generator = new WebhookGenerator(document, dataClasses);
        
        var webhooks = generator.GenerateWebhooks("PaymentApiV1");
        var interfaceCode = webhooks["IPaymentAPIWebhookHandler"];
        
        interfaceCode.ShouldContain("Notifies when a payment is completed successfully");
        interfaceCode.ShouldContain("Notifies when a payment fails");
        interfaceCode.ShouldContain("Notifies when a refund is processed");
    }

    [Fact]
    public void Should_ReturnEmptyForNoWebhooks()
    {
        var json = @"{
            ""openapi"": ""3.1.0"",
            ""info"": {
                ""title"": ""Simple API"",
                ""version"": ""1.0.0""
            },
            ""paths"": {
                ""/test"": {
                    ""get"": {
                        ""responses"": {
                            ""200"": {
                                ""description"": ""Success""
                            }
                        }
                    }
                }
            }
        }";
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var document = JsonSerializer.Deserialize<OpenApiDocument>(json, options)!;
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var generator = new WebhookGenerator(document, dataClasses);
        
        var webhooks = generator.GenerateWebhooks("SimpleApiV1");
        
        webhooks.ShouldNotBeNull();
        webhooks.ShouldBeEmpty();
    }

    [Fact]
    public void Should_UseCorrectNamespaceInWebhooks()
    {
        var document = LoadPaymentWebhooksSpec();
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var generator = new WebhookGenerator(document, dataClasses);
        
        var webhooks = generator.GenerateWebhooks("PaymentApiV1");
        
        var interfaceCode = webhooks["IPaymentAPIWebhookHandler"];
        interfaceCode.ShouldContain("using PaymentApiV1.Models;");
        interfaceCode.ShouldContain("namespace PaymentApiV1;");
        
        var handlerCode = webhooks["PaymentAPIWebhookHandler"];
        handlerCode.ShouldContain("using PaymentApiV1.Models;");
        handlerCode.ShouldContain("namespace PaymentApiV1;");
    }

    [Fact]
    public void Should_GenerateVirtualMethodsInHandler()
    {
        var document = LoadPaymentWebhooksSpec();
        var dataClasses = new DataClassGenerator(document).GenerateDataClasses();
        var generator = new WebhookGenerator(document, dataClasses);
        
        var webhooks = generator.GenerateWebhooks("PaymentApiV1");
        var handlerCode = webhooks["PaymentAPIWebhookHandler"];
        
        // Methods should be virtual to allow overriding
        handlerCode.ShouldContain("public virtual Task");
        // Should have default implementation
        handlerCode.ShouldContain("Console.WriteLine");
        handlerCode.ShouldContain("return Task.CompletedTask;");
    }
}
