using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SevillaDotNetMCPServer.Tools
{
    [McpServerToolType]
    public class UpcomingEventsTool
    {
        /// <summary>
        /// Simulates retrieving a list of upcoming .NET community events with progress tracking.
        /// </summary>
        /// <param name="server">MCP server instance used for notifications.</param>
        /// <param name="context">Request context providing metadata such as progress tracking tokens.</param>
        /// <param name="duration">Total duration of the simulated operation in seconds (default: 10).</param>
        /// <param name="steps">Number of progress updates before completion (default: 5).</param>
        /// <returns>A formatted string containing fake upcoming .NET events.</returns>
        public static async Task<string> GetUpcomingEvents(
            [Description("MCP server instance handling the request.")]
            IMcpServer server,

            [Description("Request context with metadata like progress tracking.")]
            RequestContext<CallToolRequestParams> context,

            [Description("Total duration of the operation in seconds. Default: 10.")]
            int duration = 10,

            [Description("Number of progress steps before completion. Default: 5.")]
            int steps = 5)
        {
            var progressToken = context.Params?.Meta?.ProgressToken;
            var stepDuration = duration / steps;

            try
            {
                Console.WriteLine("Fetching upcoming events...");

                // Simulate progress updates while retrieving events
                for (int i = 1; i <= steps; i++)
                {
                    await Task.Delay(stepDuration * 1000); // Simulated delay

                    if (progressToken is not null)
                    {
                        await server.SendNotificationAsync("notifications/progress", new
                        {
                            Progress = i,
                            Total = steps,
                            progressToken
                        });
                    }
                }

                // Fake list of upcoming .NET events
                var events = new List<string>
            {
                "Hands-On Workshop: Exploring ASP.NET Core (June 15, 2025)",
                "Hands-On Workshop: Blazor & WebAssembly (September 20, 2025)",
                ".NET MAUI & Cross-Platform Development (October 12, 2025)",
                "Expert Panel on Microservices with .NET (November 20, 2025)",
                "Debugging & Performance Tuning in .NET (December 14, 2025)"
            };

                // Format event list as a single response string
                var response = "Upcoming SevillaDotNet Community Events:\n" + string.Join("\n", events);

                Console.WriteLine("Events successfully retrieved.");

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving events: {ex.Message}");

                // Log inner exception details if available
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Return a user-friendly error message
                return "An error occurred while retrieving upcoming events. Please try again later.";
            }
        }
    }
}