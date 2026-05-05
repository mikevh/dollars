using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var modelId = "";
var apiKey = "";
var endpoint = "";

var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Warning));

Kernel kernel = builder.Build();

kernel.Plugins.AddFromType<LightsPlugin>();
var ccs = kernel.GetRequiredService<IChatCompletionService>();


var settings = new OpenAIPromptExecutionSettings
{
  FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

var history = new ChatHistory();
string? userInput;
Console.WriteLine(">");
while(true)
{
    userInput = Console.ReadLine();
    if(string.IsNullOrEmpty(userInput)) break;

    history.AddUserMessage(userInput);

    var result = await ccs.GetChatMessageContentAsync(history, 
        executionSettings: settings,
        kernel: kernel);
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"AI: {result.Content}");
    Console.ForegroundColor = ConsoleColor.DarkGray;
}

public class LightsPlugin
{
    private readonly List<LightModel> lights = new () {
        new () { Id = 1, Name = "Front", IsOn = true },
        new () { Id = 2, Name = "Back", IsOn = true },
        new () { Id = 3, Name = "Bedroom" }
    };

    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    [return: Description("An array of lights")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        return lights;
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of a light")]
    [return: Description("The updated state of the light; will return null if the light does not exist")]
    public async Task<LightModel?> ChangeStateAsync(
        [Description("The identifier of the light to change")]
        int id, 
        [Description("True if the light should be turned on, false to turn it off")]
        bool isOn)
    {
        var light = lights.FirstOrDefault(l => l.Id == id);
        light?.IsOn = isOn;

        return light;
    }
}

public class LightModel()
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOn { get; set;}
}