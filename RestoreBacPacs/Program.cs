using CommandLine;
using RestoreBacPacs;

internal class Program
{
    private async static Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var parsedArguments = Parser.Default.ParseArguments<NamedParameters>(args);

        await parsedArguments.WithParsedAsync(opts => BeginWorkAsync(opts, cancellationTokenSource.Token));
        parsedArguments.WithNotParsed<NamedParameters>(errors => 
            {
            Console.WriteLine("Unable to parse the following parameters");
            foreach (var error in errors)
            {
                Console.WriteLine(error.ToString());
            }
        });
    }

    private static async Task BeginWorkAsync(NamedParameters namedParams, CancellationToken token)
    {
        var username = Environment.GetEnvironmentVariable("USERNAME");
        string filePath = @"C:\users\{0}\AppData\Local\Temp\Gsp.DevTools\Bacpacs\{1}";
        var bacPacs = Directory.GetFiles(string.Format(filePath, username, namedParams.Environment));
        var filteredBacPacs = bacPacs.Where(x => !x.Contains("-patched")).ToList();
        if (!namedParams.DatabaseName.Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
            filteredBacPacs = filteredBacPacs.Where(x => x.Contains(namedParams.DatabaseName, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }
        var db = new DatabaseFacade(namedParams.DbServer, namedParams.DbLogin, namedParams.DbPassword);
        foreach (var bacPac in filteredBacPacs)
        {
            token.ThrowIfCancellationRequested();
            var fileName = Path.GetFileName(bacPac).Split('.')[0];

            Console.WriteLine("Closing connection to {0}", fileName);
            await db.CloseConnectionAsync(fileName, token);

            Console.WriteLine("Dropping {0} if exists", fileName);
            await db.DropDatabaseAsync(fileName, token);

            Console.WriteLine("Restoring {0}", fileName);
            await db.RestoreDatabaseUsingBacpacAsync(bacPac, token);

        }
        Console.WriteLine("Done importing bacpacs.");
        Console.WriteLine("Press whichever key you like to exit, or don't and just sit there like an idiot staring at the terminal.");
        Console.ReadLine();
    }
}