using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

Console.WriteLine("SevillaDotNet MCP Client");
Console.WriteLine("=========================================");

try
{
    Console.WriteLine("Introduce your Azure MCP Server project path:");
    var projectPath = Console.ReadLine();

    if (string.IsNullOrEmpty(projectPath))
    {
        Console.WriteLine("Invalid project path. Exiting...");
        return;
    }

    Console.WriteLine("Initializing client transport...");

    // Define client options with metadata
    var clientOptions = new McpClientOptions
    {
        ClientInfo = new Implementation
        {
            Name = "SevillaDotNet Client",
            Version = "1.0.0"
        }
    }; 
    
    // Configure transport options for connecting to the server
    var transportOptions = new StdioClientTransportOptions
    {
        Name = "SevillaDotNet MCP Server",
        Command = "dotnet",
        Arguments = ["run", "--project", projectPath, "--no-build"]
    };

    Console.WriteLine("Creating transport...");
    var transport = new StdioClientTransport(transportOptions);

    Console.WriteLine("Attempting to connect to server...");

    // Define a cancellation token to limit the connection attempt duration
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

    try
    {
        // Establish client connection with transport options
        IMcpClient client = await McpClientFactory.CreateAsync(transport, clientOptions, cancellationToken: cts.Token);

        Console.WriteLine("✅ Connected successfully!");

        // Retrieve the list of available tools from the server
        Console.WriteLine("Fetching available tools...");
        IList<McpClientTool> tools = await client.ListToolsAsync(null, cts.Token);

        Console.WriteLine($"🔧 {tools.Count} tools found:");

        foreach (McpClientTool tool in tools)
        {
            Console.WriteLine($"- {tool.Name}: {tool.Description ?? "No description provided"}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Connection error: {ex.Message}");

        // Log inner exception details if available
        if (ex.InnerException != null)
        {
            Console.WriteLine($"🔎 Inner exception: {ex.InnerException.Message}");
        }
    }
    finally
    {
        Console.WriteLine("Client connection attempt completed.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"🚨 Fatal error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();