using System.Diagnostics;
using System.Text.Json;
using OpenApiToCS.Generator;
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

var dataClassesTask = DataClassGenerator.GenerateDataClasses(obj);
var apiClassesTask = OperationGenerator.GenerateApiClasses(obj);

Directory.CreateDirectory("code/Models");
Directory.CreateDirectory("code/Api");

foreach (var dataClass in dataClassesTask)
{
    File.WriteAllText("code/Models/" + dataClass.Key + ".cs", dataClass.Value);
}

foreach (var apiClass in apiClassesTask)
{
    File.WriteAllText("code/Api/" + apiClass.Key + ".cs", apiClass.Value);
}

Console.WriteLine($"Generated {dataClassesTask.Count} data classes and {apiClassesTask.Count} API classes in {stopwatch.ElapsedMilliseconds} ms");

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