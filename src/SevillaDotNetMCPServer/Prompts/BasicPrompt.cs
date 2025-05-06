using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SevillaDotNetMCPServer.Prompts
{
    [McpServerPromptType]
    public class BasicPrompt
    {
        /// <summary>
        /// Generates a basic prompt requesting a summary of SevillaDotNet.
        /// SevillaDotNet is a .NET user group based in Seville, Spain, dedicated to fostering knowledge-sharing,
        /// organizing meetups, workshops, and discussions related to .NET technologies.
        /// This prompt serves as a simple request for an overview of the group's mission, activities, and recent events.
        /// </summary>
        /// <returns>A predefined string requesting a summary of SevillaDotNet.</returns>
        [McpServerPrompt(Name = "SevillaDotNetBasicPrompt"), Description("A simple prompt without arguments about SevillaDotNet.")]
        public static string GetBasicPrompt() => "Provide a detailed summary of SevillaDotNet, the .NET user group based in Seville, Spain.";
    }
}