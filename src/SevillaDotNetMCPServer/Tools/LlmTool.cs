using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SevillaDotNetMCPServer.Tools
{
    [McpServerToolType]
    public class LlmTool
    {
        /// <summary>
        /// Retrieves a summary of the SevillaDotNet community.
        /// This summary includes its mission, activities, and highlights from recent events.
        /// SevillaDotNet is a .NET user group based in Seville, Spain, dedicated to promoting learning,
        /// networking, and the latest advancements in the .NET ecosystem. The group regularly hosts meetups,
        /// workshops, and expert panels to share knowledge and foster collaboration.
        /// </summary>
        /// <param name="server">MCP server instance handling the request.</param>
        /// <param name="prompt">The prompt used to generate the summary via the language model.</param>
        /// <param name="maxTokens">Defines the maximum number of tokens the response should contain to limit content length.</param>
        [McpServerTool(Name = "getSevillaDotNetSummary"), Description("Retrieves a summary of the SevillaDotNet community, including its mission and recent events.")]
        public static async Task<string> GetSevillaDotNetSummary(
            IMcpServer server,
            [Description("The prompt to send to the LLM")] string prompt,
            [Description("Maximum number of tokens to generate")] int maxTokens,
            CancellationToken cancellationToken)
        {
            string defaultPrompt = @"
            Provide a detailed summary of SevillaDotNet, the .NET user group based in Seville, Spain. 
            Include key information about its mission, activities, and how it engages with the developer community. 
            Additionally, summarize its most recent events, including key topics, speakers, and any notable takeaways. 
            Highlight how the group contributes to the .NET ecosystem.";

            var samplingParams = CreateRequestSamplingParams(prompt ?? defaultPrompt, maxTokens);
            var sampleResult = await server.RequestSamplingAsync(samplingParams, cancellationToken);

            return sampleResult.Content.Text;
        }

        static CreateMessageRequestParams CreateRequestSamplingParams(string prompt, int maxTokens = 2000)
        {
            return new CreateMessageRequestParams()
            {
                Messages = [new SamplingMessage()
                {
                    Role = Role.User,
                    Content = new Content()
                    {
                        Type = "text",
                        Text = prompt
                    }
                }],
                SystemPrompt = "You are a helpful server with information about .NET communities.",
                MaxTokens = maxTokens,
                Temperature = 0.7f,
                IncludeContext = ContextInclusion.ThisServer
            };
        }
    }
}