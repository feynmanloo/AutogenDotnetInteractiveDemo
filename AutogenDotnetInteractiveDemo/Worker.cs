using AutoGen.Core;
using AutoGen.DotnetInteractive;
using AutoGen.SemanticKernel.Extension;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace AutogenDotnetInteractiveDemo;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string openAiKey = "";
        string modelId = "llama3";
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0
        };
        var coderAgent = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: modelId, apiKey: openAiKey, httpClient: new HttpClient(new OpenAiHttpClientHandler()))
            .Build()
            .ToSemanticKernelAgent(name: "coder", systemMessage:  @"You act as dotnet coder, you write dotnet code to resolve task. Once you finish writing code, ask runner to run the code for you.

Here're some rules to follow on writing dotnet code:
- put code between ```csharp and ```
- When creating http client, use `var httpClient = new HttpClient()`. Don't use `using var httpClient = new HttpClient()` because it will cause error when running the code.
- Try to use `var` instead of explicit type.
- Try avoid using external library, use .NET Core library instead.
- Use top level statement to write code.
- Always print out the result to console. Don't write code that doesn't print out anything.

If you need to install nuget packages, put nuget packages in the following format:
```nuget
nuget_package_name
```

If your code is incorrect, Fix the error and send the code again.

Here's some externel information
- The link to mlnet repo is: https://github.com/dotnet/machinelearning. you don't need a token to use github pr api. Make sure to include a User-Agent header, otherwise github will reject it.
", settings)
            .RegisterPrintMessage();
        var workingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(workingDirectory);
        var interactiveService = new InteractiveService(installingDirectory: workingDirectory);
        await interactiveService.StartAsync(workingDirectory: workingDirectory, ct: stoppingToken);
        // register dotnet code block execution hook to an arbitrary agent
        var dotnetCodeAgent = coderAgent.RegisterDotnetCodeBlockExectionHook(interactiveService: interactiveService);

        var codeSnippet = @"
```csharp
Console.WriteLine(""Hello World"");
```";

        await dotnetCodeAgent.SendAsync(codeSnippet, ct: stoppingToken);
        // output: Hello World

        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     if (_logger.IsEnabled(LogLevel.Information))
        //     {
        //         _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //     }
        //
        //     await Task.Delay(1000, stoppingToken);
        // }
        _logger.LogInformation("Ending worker...");
    }
}