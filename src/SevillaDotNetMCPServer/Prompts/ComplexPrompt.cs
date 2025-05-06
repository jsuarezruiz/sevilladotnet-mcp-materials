using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using SevillaDotNetMCPServer.Tools;
using System.ComponentModel;

namespace SevillaDotNetMCPServer.Prompts
{
    [McpServerPromptType]
    public class ComplexPrompt
    {
        /// <summary>
        /// Generates a complex prompt related to SevillaDotNet events for a specific year.
        /// This method creates contextualized messages that can be used to request information
        /// about past or upcoming SevillaDotNet events, including key topics, speakers, and community impact.
        /// </summary>
        /// <param name="year">The target year for generating the SevillaDotNet-related prompt.</param>
        /// <returns>A collection of ChatMessage objects forming a structured prompt.</returns>
        [McpServerPrompt(Name = "SevillaDotNetComplexPrompt"), Description("A complex prompt related to SevillaDotNet events for a given year.")]
        public static IEnumerable<ChatMessage> GetComplexPrompt(
            [Description("The target year for generating the SevillaDotNet-related prompt.")]
            int year)
        {
            return [
                new ChatMessage(ChatRole.User,
                            "Provide a detailed summary of SevillaDotNet, the .NET user group based in Seville, Spain." +
                            "Include key information about its mission, activities, and how it engages with the developer community." +
                            $"Additionally, summarize its {year} events, including key topics, speakers, and any notable takeaways."),
                            new ChatMessage(ChatRole.Assistant,
                            "I understand. You've provided a complex prompt with the SevillaDotNet year argument. How would you like me to proceed?"),
                            new ChatMessage(ChatRole.User, [new DataContent(LogoTool.MCP_SEVILLADOTNET_LOGO)])
            ];
        }
    }
}