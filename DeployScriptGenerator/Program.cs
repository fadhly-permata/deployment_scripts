using System.Data;
using DeployScriptGenerator.Utilities.Constants;
using DeployScriptGenerator.Utilities.Models;
using IDX.Utilities;
using IDX.Utilities.DataProcessor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static DeployScriptGenerator.Utilities.Models.Program;

namespace DeployScriptGenerator;

internal partial class Program
{
    private static string? currentDbName;

    private static void Main(string[] args)
    {
        ShowWelcomeMessage();
    }

    static void ShowWelcomeMessage()
    {
        restart_process:
        Console.Clear();
        Console.WriteLine(ConstMessages.CMD_MSG_WELCOME);

        try
        {
            if (
                int.TryParse(Console.ReadLine(), out var userResponse)
                && userResponse is >= 1 and <= 3
            )
            {
                switch (userResponse)
                {
                    case 1:
                        GenerateSampleJsonConfigs();
                        break;
                    case 2:
                        GenerateDeployScript();
                        break;
                    case 3:
                        Console.WriteLine(ConstMessages.EXITING);
                        break;
                }
            }
            else
            {
                Console.WriteLine(ConstMessages.INVALID_RESPONSE, userResponse);
                goto restart_process;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message} @ {ex.StackTrace}");
            Console.WriteLine("Press [ENTER] key to restart the application.");
            Console.Read();
            goto restart_process;
        }
    }

    static void GenerateDeployScript()
    {
        Console.WriteLine("Generating deploy script...");
        var configJson = ScriptPreparation();

        Console.WriteLine($"Connecting to database \"{GetDbName(configJson!)!}\"...");
        var pghelper = DBConnection(configJson);
        if (pghelper is null)
        {
            Console.WriteLine("Failed to create database connection.");
            return;
        }

        Console.WriteLine(
            $"Connection to database to \"{GetDbName(configJson!)!}\" has been established."
        );

        currentDbName = GetDbName(configJson!);
        DDLFetchTables(pghelper, configJson!);

        Console.WriteLine(
            $"Deployment Scripts has been generated on Path: {configJson!.OutputDirectory}"
        );

        Console.WriteLine("Press [Enter] key to go back to the main menu.");
        Console.Read();
        ShowWelcomeMessage();
    }

    static string? GetDbName(ConfigurationModel configJson) =>
        configJson
            .ConnectionString.Split(';')
            .FirstOrDefault(x => x.Contains("Database"))
            ?.Split('=')[1];

    static string FilenameBuilder(ScriptDataModel sdm) =>
        $"{sdm.Database}_{sdm.Schema}_{sdm.ObjectName}_{sdm.ConfigJson.TicketNumber}{sdm.OperationType()}.sql";

    static void WriteScript(string filename, string script)
    {
        var dir = Path.GetDirectoryName(filename);

        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(filename, script);
    }

    static void DDLFetchTables(PgSqlHelper pghelper, ConfigurationModel configJson)
    {
        Console.WriteLine("Fetching table DDLs...");
        configJson
            .FetchDDL?.Tables?.ToList()
            .ForEach(
                (TableModel table) =>
                {
                    Console.WriteLine(
                        $"Processing {table.Database}.{table.Schema}.{table.Table}..."
                    );

                    if (currentDbName != table.Database)
                    {
                        Console.WriteLine($"Switching to database {table.Database}...");

                        pghelper.ChangeDB(table.Database);
                        currentDbName = table.Database;
                    }

                    ExecAndSaveScalar(
                        pghelper: pghelper,
                        table: table,
                        configJson: configJson,
                        onNullMessage: $"Table [{table.Database}.{table.Schema}.{table.Table}] does not exist.",
                        type: ScriptDataModel.ScriptType.Table,
                        query: ConstQueries.FETCH_TABLES.BindWith(
                            new Dictionary<string, object>
                            {
                                { "{{schema_name}}", table.Schema },
                                { "{{table_name}}", table.Table }
                            }
                        )
                    );

                    if (table.FetchConstraints == true)
                        ExecAndSaveTableProperties(
                            pghelper: pghelper,
                            table: table,
                            configJson: configJson,
                            type: ScriptDataModel.ScriptType.Constraint,
                            query: ConstQueries.FETCH_TABLES_CONSTRAINS.BindWith(
                                new Dictionary<string, object>
                                {
                                    { "{{schema_name}}", table.Schema },
                                    { "{{table_name}}", table.Table }
                                }
                            )
                        );

                    if (table.FetchIndexes == true)
                        ExecAndSaveTableProperties(
                            pghelper: pghelper,
                            table: table,
                            configJson: configJson,
                            type: ScriptDataModel.ScriptType.Index,
                            query: ConstQueries.FETCH_TABLES_INDEXES.BindWith(
                                new Dictionary<string, object>
                                {
                                    { "{{schema_name}}", table.Schema },
                                    { "{{table_name}}", table.Table }
                                }
                            )
                        );

                    Console.WriteLine(
                        $"Finished processing {table.Database}.{table.Schema}.{table.Table}."
                    );
                }
            );

        Console.WriteLine("Finished fetching table DDLs.");
    }

    private static void ExecAndSaveScalar(
        PgSqlHelper pghelper,
        TableModel table,
        ConfigurationModel configJson,
        ScriptDataModel.ScriptType type,
        string onNullMessage,
        string query
    )
    {
        pghelper.ExecuteScalar(query, out string? result);

        if (string.IsNullOrWhiteSpace(result))
        {
            Console.WriteLine(onNullMessage);
            return;
        }

        WriteScript(
            script: result,
            filename: Path.Combine(
                path1: configJson.OutputDirectory,
                path2: table.Database,
                path3: table.Schema,
                path4: FilenameBuilder(
                    new ScriptDataModel
                    {
                        Type = type,
                        ConfigJson = configJson,
                        ObjectName = table.Table,
                        Schema = table.Schema,
                        Database = table.Database
                    }
                )
            )
        );
    }

    private static void ExecAndSaveTableProperties(
        PgSqlHelper pghelper,
        TableModel table,
        ConfigurationModel configJson,
        ScriptDataModel.ScriptType type,
        string query
    )
    {
        Console.WriteLine(
            $"Fetching {Enum.GetName(type)}s for table \"{table.Database}.{table.Schema}.{table.Table}\"..."
        );
        pghelper.ExecuteQuery(query, out var result);

        if (result is null)
        {
            Console.WriteLine(
                $"No {Enum.GetName(type)} found for table \"{table.Database}.{table.Schema}.{table.Table}\"."
            );
            return;
        }

        result.ForEach(x =>
        {
            var jo = JObject.FromObject(x);
            Console.WriteLine(
                $"Processing {Enum.GetName(type)} \"{jo["name"].ToString()}\" for table \"{table.Database}.{table.Schema}.{table.Table}\"."
            );

            WriteScript(
                script: jo["script"].ToString(),
                filename: Path.Combine(
                    path1: configJson.OutputDirectory,
                    path2: table.Database,
                    path3: table.Schema,
                    path4: FilenameBuilder(
                        new ScriptDataModel
                        {
                            Type = type,
                            ConfigJson = configJson,
                            ObjectName = jo["name"].ToString(),
                            Schema = table.Schema,
                            Database = table.Database
                        }
                    )
                )
            );
        });
    }

    static ConfigurationModel? ScriptPreparation()
    {
        restart_process:

        try
        {
            Console.WriteLine("Please specify the location of the json configuration file:");

            var userResponse = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userResponse))
                throw new ArgumentException("Configuration file path can not be empty.");

            if (File.Exists(userResponse) == false)
                throw new FileNotFoundException("The specified file does not exist.");

            var configFileContent = File.ReadAllText(userResponse!);
            var configJson =
                JsonConvert.DeserializeObject<ConfigurationModel>(configFileContent)
                ?? throw new DataException("The specified file could not be parsed.");

            return configJson;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(
                "Press <r> to try again or press any other key to go back to the main menu?"
            );
            if (Console.ReadLine()?.ToLower() == "r")
                goto restart_process;
            else
                ShowWelcomeMessage();

            return default;
        }
    }

    static PgSqlHelper? DBConnection(ConfigurationModel? configJson)
    {
        try
        {
            return new PgSqlHelper(configJson!.ConnectionString).Connect(
                useTransaction: false,
                disconnectFirst: true
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message} @ {ex.StackTrace}");
            return default;
        }
    }
}
