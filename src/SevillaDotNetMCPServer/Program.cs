using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SevillaDotNetMCPServer;
using SevillaDotNetMCPServer.Resources;
using SevillaDotNetMCPServer.Tools;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

// ===== APPLICATION SETUP =====
// Creates a new host builder for the MCP server application
var builder = Host.CreateApplicationBuilder(args);

// Configure console logging to send all logs to standard error instead of standard output
// This is useful for containerized applications and separating logs from other output
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Initialize a collection to track resource subscriptions
// Subscriptions allow clients to receive updates when resources change
HashSet<string> subscriptions = [];

// Set the default minimum logging level for the application
// This can be changed at runtime through the SetLoggingLevelHandler
var _minimumLoggingLevel = LoggingLevel.Debug;

// ===== MCP SERVER CONFIGURATION =====
// Configure the MCP server with all required handlers and tools
builder.Services
    // Register core MCP server services
    .AddMcpServer()
    // Use standard input/output for communication with clients
    // This enables the server to be used as a child process or in CLI scenarios
    .WithStdioServerTransport()
    // Register custom tools that clients can access
    // Each tool provides specific functionality to the client
    .WithTools<EchoTool>()       // Simple echo tool for testing
    .WithTools<LlmTool>()        // Integration with language models
    .WithTools<LogoTool>()       // Tool for generating/handling logos
    .WithTools<UpcomingEventsTool>() // Tool to retrieve upcoming events data

    // ===== RESOURCE MANAGEMENT HANDLERS =====
    // Handler to list available resources to clients
    // Resources are external data that can be accessed via URI
    .WithListResourcesHandler(async (ctx, ct) =>
    {
        return new ListResourcesResult
        {
            Resources =
            [
                // Define a direct text resource that doesn't require parameters
                new ModelContextProtocol.Protocol.Types.Resource {
                    Name = "Direct Text Resource",
                    Description = "A direct text resource",
                    MimeType = "text/plain",
                    Uri = "test://direct/text/resource"
                },
            ]
        };
    })

    // Handler to list resource templates
    // Templates define resources that require parameters in their URIs
    .WithListResourceTemplatesHandler(async (ctx, ct) =>
    {
        return new ListResourceTemplatesResult
        {
            ResourceTemplates =
            [
                // Define a template resource that requires an ID parameter
                new ResourceTemplate {
                    Name = "Template Resource",
                    Description = "A template resource with a numeric ID",
                    UriTemplate = "test://template/resource/{id}"
                }
            ]
        };
    })

    // Handler to read the contents of a resource by its URI
    // This is how clients actually access the resource data
    .WithReadResourceHandler(async (ctx, ct) =>
    {
        var uri = ctx.Params?.Uri;

        // Handle direct resource access
        if (uri == "test://direct/text/resource")
        {
            return new ReadResourceResult
            {
                Contents = [new TextResourceContents
                {
                    Text = "This is a direct resource",
                    MimeType = "text/plain",
                    Uri = uri,
                }]
            };
        }

        // Handle template resources with parameters
        if (uri is null || !uri.StartsWith("test://template/resource/"))
        {
            throw new NotSupportedException($"Unknown resource: {uri}");
        }

        // Extract the ID from the URI and use it to find the resource
        int index = int.Parse(uri["test://template/resource/".Length..]) - 1;

        if (index < 0 || index >= ResourceGenerator.Resources.Count)
        {
            throw new NotSupportedException($"Unknown resource: {uri}");
        }

        var resource = ResourceGenerator.Resources[index];

        // Return the resource contents based on its MIME type
        // Text resources are returned as TextResourceContents
        if (resource.MimeType == "text/plain")
        {
            return new ReadResourceResult
            {
                Contents = [new TextResourceContents
                {
                    Text = resource.Description!,
                    MimeType = resource.MimeType,
                    Uri = resource.Uri,
                }]
            };
        }
        // Binary resources (images, etc.) are returned as BlobResourceContents
        else
        {
            return new ReadResourceResult
            {
                Contents = [new BlobResourceContents
                {
                    Blob = resource.Description!,
                    MimeType = resource.MimeType,
                    Uri = resource.Uri,
                }]
            };
        }
    })

    // ===== SUBSCRIPTION HANDLERS =====
    // Handler to subscribe to resource updates
    // Clients use this to be notified when resources change
    .WithSubscribeToResourcesHandler(async (ctx, ct) =>
    {
        var uri = ctx.Params?.Uri;

        if (uri is not null)
        {
            // Add the resource URI to the subscription list
            subscriptions.Add(uri);

            // Demonstrate using the language model to acknowledge the subscription
            // This shows how to use the MCP server's own LLM capabilities
            await ctx.Server.RequestSamplingAsync([
                new ChatMessage(ChatRole.System, "You are a helpful test server"),
                new ChatMessage(ChatRole.User, $"Resource {uri}, context: A new subscription was started"),
            ],
            options: new ChatOptions
            {
                MaxOutputTokens = 100,
                Temperature = 0.7f,
            },
            cancellationToken: ct);
        }

        return new EmptyResult();
    })

    // Handler to unsubscribe from resource updates
    .WithUnsubscribeFromResourcesHandler(async (ctx, ct) =>
    {
        var uri = ctx.Params?.Uri;
        if (uri is not null)
        {
            // Remove the resource URI from the subscription list
            subscriptions.Remove(uri);
        }
        return new EmptyResult();
    })

    // ===== AUTOCOMPLETE HANDLER =====
    // Handler for providing completion suggestions for arguments
    // This enables better client experiences with autocomplete
    .WithCompleteHandler(async (ctx, ct) =>
    {
        // Define possible completions for different argument types
        var exampleCompletions = new Dictionary<string, IEnumerable<string>>
        {
            { "style", ["casual", "formal", "technical", "friendly"] },
            { "temperature", ["0", "0.5", "0.7", "1.0"] },
            { "resourceId", ["1", "2", "3", "4", "5"] }
        };

        if (ctx.Params is not { } @params)
        {
            throw new NotSupportedException($"Params are required.");
        }

        var @ref = @params.Ref;
        var argument = @params.Argument;

        // Handle completions for resource references
        if (@ref.Type == "ref/resource")
        {
            var resourceId = @ref.Uri?.Split("/").Last();

            if (resourceId is null)
            {
                return new CompleteResult();
            }

            // Filter resource IDs that match the partial input
            var values = exampleCompletions["resourceId"].Where(id => id.StartsWith(argument.Value));

            return new CompleteResult
            {
                Completion = new Completion { Values = [.. values], HasMore = false, Total = values.Count() }
            };
        }

        // Handle completions for prompt arguments
        if (@ref.Type == "ref/prompt")
        {
            if (!exampleCompletions.TryGetValue(argument.Name, out IEnumerable<string>? value))
            {
                throw new NotSupportedException($"Unknown argument name: {argument.Name}");
            }

            // Filter values that match the partial input
            var values = value.Where(value => value.StartsWith(argument.Value));
            return new CompleteResult
            {
                Completion = new Completion { Values = [.. values], HasMore = false, Total = values.Count() }
            };
        }

        throw new NotSupportedException($"Unknown reference type: {@ref.Type}");
    })

    // ===== LOGGING LEVEL HANDLER =====
    // Handler to change the logging level at runtime
    // This allows dynamic control of verbosity without restarting
    .WithSetLoggingLevelHandler(async (ctx, ct) =>
    {
        if (ctx.Params?.Level is null)
        {
            throw new McpException("Missing required argument 'level'", McpErrorCode.InvalidParams);
        }

        // Update the minimum logging level
        _minimumLoggingLevel = ctx.Params.Level;

        // Send a notification to confirm the change
        await ctx.Server.SendNotificationAsync("notifications/message", new
        {
            Level = "debug",
            Logger = "test-server",
            Data = $"Logging level set to {_minimumLoggingLevel}",
        }, cancellationToken: ct);

        return new EmptyResult();
    });

// ===== TELEMETRY CONFIGURATION =====
// Configure OpenTelemetry for observability
ResourceBuilder resource = ResourceBuilder.CreateDefault().AddService("everything-server");

builder.Services.AddOpenTelemetry()
    // Configure tracing with sources, HTTP client instrumentation
    .WithTracing(b => b.AddSource("*").AddHttpClientInstrumentation().SetResourceBuilder(resource))
    // Configure metrics with meters, HTTP client instrumentation
    .WithMetrics(b => b.AddMeter("*").AddHttpClientInstrumentation().SetResourceBuilder(resource))
    // Configure logging with resource builder
    .WithLogging(b => b.SetResourceBuilder(resource))
    // Export telemetry data using OpenTelemetry Protocol (OTLP)
    .UseOtlpExporter();

// ===== ADDITIONAL SERVICES =====
// Register subscription collection as singleton for dependency injection
builder.Services.AddSingleton(subscriptions);
// Register background services for handling subscriptions and logging
builder.Services.AddHostedService<SubscriptionMessageSender>();
builder.Services.AddHostedService<LoggingUpdateMessageSender>();

// Register function to access current logging level
builder.Services.AddSingleton<Func<LoggingLevel>>(_ => () => _minimumLoggingLevel);

// Build and run the host
await builder.Build().RunAsync();