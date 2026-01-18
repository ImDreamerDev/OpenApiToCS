namespace OpenApiToCS;

public static class ArgumentParser
{
    public static CliOptions Parse(string[] args)
    {
        var options = new CliOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--help" or "-h":
                    options.ShowHelp = true;
                    return options;
                
                case "--version" or "-v":
                    options.ShowVersion = true;
                    return options;
                
                case "--output" or "-o":
                    options.OutputDirectory = GetNextArg(args, ref i);
                    break;
                
                case "--config" or "-c":
                    options.ConfigFile = GetNextArg(args, ref i);
                    break;
                
                case "--namespace" or "-n":
                    options.Namespace = GetNextArg(args, ref i);
                    break;
                
                case "--template-dir" or "-t":
                    options.TemplateDirectory = GetNextArg(args, ref i);
                    break;
                
                case "--mock-output":
                    options.MockServerOutput = GetNextArg(args, ref i);
                    break;
                
                case "--watch" or "-w":
                    options.WatchMode = true;
                    break;
                
                case "--verbose":
                    options.Verbose = true;
                    break;
                
                case "--validate":
                    options.Validate = true;
                    break;
                
                case "--analyze":
                    options.Analyze = true;
                    break;
                
                case "--generate-mock-server" or "--mock-server":
                    options.GenerateMockServer = true;
                    break;
                
                case "--mono":
                    options.GenerateMonoClients = true;
                    break;
                
                default:
                    if (!arg.StartsWith("-") && string.IsNullOrEmpty(options.InputFile))
                    {
                        options.InputFile = arg;
                    }
                    break;
            }
        }

        return options;
    }

    private static string GetNextArg(string[] args, ref int i)
    {
        if (i + 1 < args.Length)
        {
            return args[++i];
        }
        return string.Empty;
    }
}
