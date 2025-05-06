using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SevillaDotNetMCPServer.Tools
{
    [McpServerToolType]
    public class EchoTool
    {
        /// <summary>
        /// Echoes the received message back to the client.
        /// Useful for testing connectivity and validating MCP request handling.
        /// </summary>
        /// <param name="message">The message sent by the client.</param>
        /// <returns>The same message prefixed with 'Echo SevillaDotNet'.</returns>
        [McpServerTool(Name = "echoSevillaDotNet"), Description("Echoes the message back to the client.")]
        public static string Echo(
            [Description("The message to be echoed back to the client.")] 
            string message) => $"Echo SevillaDotNet: {message}";
    }
}
