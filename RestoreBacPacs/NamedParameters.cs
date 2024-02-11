using CommandLine;

namespace RestoreBacPacs;

public class NamedParameters
{
    [Option('e', "environment", Required = false, Default = "test", HelpText = "Specify which environment you wish to restore")]
    public required string Environment { get; set; }

    [Option('d', "dbname", Required = false, Default = "all", HelpText = "Which database do you wish to restore?, if ignored all will be restored.")]
    public required string DatabaseName { get; set; }

    [Option('s', "db-server", Required = false, Default = "localhost.localdomain", HelpText = "Specify your uri to the dbserver")]
    public required string DbServer { get; set; }
    
    [Option('l', "db-login", Required = true, HelpText = "Specify your login to the dbserver")]
    public required string DbLogin { get; set; }
    
    [Option('p', "db-password", Required = true, HelpText = "Specify your password to the dbserver")]
    public required string DbPassword { get; set; }
}