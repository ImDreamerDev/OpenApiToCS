using System.Diagnostics;
using System.Text.Json;
using OpenApiToCS.Generator;
using OpenApiToCS.Generator.Models;
using OpenApiToCS.OpenApi;
Stopwatch stopwatch = Stopwatch.StartNew();

if (args.Length == 0)
{
    Console.Error.WriteLine("Please provide the path to the OpenAPI JSON file as an argument.");
}
string apiPath = args[0];

JsonSerializerOptions serializationOptions = new JsonSerializerOptions
{
    TypeInfoResolver = Extensions.OpenApiSourceGenerationContext.Default
};

OpenApiDocument? obj = JsonSerializer.Deserialize<OpenApiDocument>(File.ReadAllText(apiPath), serializationOptions);

if (obj is null)
{
    Console.Error.WriteLine("Failed to deserialize the OpenAPI document. Please check the file format.");
    return;
}

var dataClasses = new DataClassGenerator().GenerateDataClasses(obj);
var apiClasses = new OperationGenerator(dataClasses).GenerateApiClasses(obj);

Directory.CreateDirectory("code/Models");
Directory.CreateDirectory("code/Api");

foreach (var dataClass in dataClasses.Classes)
{
    File.WriteAllText("code/Models/" + dataClass.Key + ".cs", dataClass.Value.Source);
    foreach (OneOfConverter oneOfConverter in dataClass.Value.OneOfConverters)
    {
        File.WriteAllText("code/Models/" + oneOfConverter.Name + ".cs", oneOfConverter.Source);
        foreach (Class oneOf in oneOfConverter.OneOfs)
        {
            File.WriteAllText("code/Models/" + oneOf.Name + ".cs", oneOf.Source);
        }
    }
}

foreach (var apiClass in apiClasses)
{
    File.WriteAllText("code/Api/" + apiClass.Key + ".cs", apiClass.Value);
}

Console.WriteLine($"Generated {dataClasses.ClassCount} data classes and {apiClasses.Count} API classes in {stopwatch.ElapsedMilliseconds} ms");
/*
stopwatch.Restart();
var result = await new CustomClientV1().GetCustom("username", "password", 10, 1, "json", DateTimeOffset.UtcNow.AddDays(-7));
Console.WriteLine($"Retrieved {result.Length} events in {stopwatch.ElapsedMilliseconds} ms");
*/
Debugger.Break();
/*

var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    (httpRequestMessage, cert, cetChain, policyErrors) =>
    {
        return true;
    };
var demo = new UserClientV1(new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5093") });

var user = await demo.GetUserId("176655424609583104");
*/