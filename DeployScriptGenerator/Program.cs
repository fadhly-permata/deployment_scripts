using System.Data;
using System.Text.RegularExpressions;
using DeployScriptGenerator.Utilities.Constants;
using DeployScriptGenerator.Utilities.Extensions.Strings;
using DeployScriptGenerator.Utilities.Models;
using IDX.Utilities;
using IDX.Utilities.DataProcessor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static DeployScriptGenerator.Utilities.Models.Program;

namespace DeployScriptGenerator;

internal partial class Program
{
    private const int EQUALS_CHAR_COUNT = 60;
    private static string? currentDbName;

    private static void Main(string[] args)
    {
        ShowWelcomeMessage();
    }

    static void ShowWelcomeMessage()
    {
        restart_process:
        Console.Clear();
        ConstMessages.CMD_MSG_WELCOME.WriteLine();

        try
        {
            if (
                int.TryParse(Console.ReadLine(), out int userResponse)
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
                        ConstMessages.CMD_MSG_EXITING.WriteLine();
                        break;
                }
            }
            else
            {
                ConstMessages.CMD_MSG_INV_RESP.WriteLine(args: userResponse);
                goto restart_process;
            }
        }
        catch (Exception ex)
        {
            ConstMessages.CMD_MSG_ERROR.WriteLine(
                args: [ex.Message, ex.StackTrace ?? string.Empty]
            );
            ConstMessages.CMD_MSG_RESTART_APP.WriteLine();
            Console.Read();
            goto restart_process;
        }
    }

    static void GenerateDeployScript()
    {
        ConstMessages.DPY_SCRIPT_START.WriteLine();
        ConfigurationModel? configJson = ScriptPreparation();

        if (configJson!.CleanupDirectoryFirst == true)
            CleanUpDirectory(configJson.OutputDirectory);

        ConstMessages.CONN_DB_TRY.WriteLine(args: GetDbName(configJson!) ?? string.Empty);
        PgSqlHelper? pghelper = DBConnection(configJson);
        if (pghelper is null)
        {
            ConstMessages.CONN_DB_FAIL.WriteLine();
            return;
        }

        ConstMessages.CONN_DB_ESTABLISHED.WriteLine(args: GetDbName(configJson!) ?? string.Empty);

        currentDbName = GetDbName(configJson: configJson!);
        DDLFetchTables(pghelper: pghelper, configJson: configJson!);
        DDLFetchFunctions(pghelper: pghelper, configJson: configJson!);

        ConstMessages.DPY_SCRIPT_GEN_SUCCESS.WriteLine(args: configJson.OutputDirectory);

        ConstMessages.CMD_MSG_RESTART_APP.WriteLine();
        Console.Read();
        ShowWelcomeMessage();
    }

    private static void CleanUpDirectory(string outputDirectory)
    {
        ConstMessages.REM_DIR.WriteLine(outputDirectory);
        if (Directory.Exists(path: outputDirectory))
            Directory.Delete(path: outputDirectory, recursive: true);
    }

    static string? GetDbName(ConfigurationModel configJson) =>
        configJson
            .ConnectionString.Split(separator: ';')
            .FirstOrDefault(static (string x) => x.Contains(value: "Database"))
            ?.Split(separator: '=')[1];

    static string FilenameBuilder(ScriptDataModel sdm) =>
        ConstMessages.FILE_NAME_BLD.Format(
            args:
            [
                sdm.Database,
                sdm.Schema,
                sdm.ObjectName,
                sdm.ParameterCount > 0 ? $"_P{sdm.ParameterCount}" : string.Empty,
                sdm.ConfigJson.TicketNumber,
                sdm.OperationType()
            ]
        );

    static void WriteScript(string filename, string script)
    {
        string? dir = Path.GetDirectoryName(path: filename);

        if (dir is not null && !Directory.Exists(path: dir))
            Directory.CreateDirectory(path: dir);

        File.WriteAllText(path: filename, contents: script);
    }

    static void DDLFetchTables(PgSqlHelper pghelper, ConfigurationModel configJson)
    {
        ConstMessages.FETCH_TBL_START.WriteLine();

        configJson
            .FetchDDL?.Tables?.ToList()
            .ForEach(
                (TableModel table) =>
                {
                    if (currentDbName != table.Database)
                    {
                        ConstMessages.CONN_DB_SWITCH.WriteLine(args: table.Database);

                        pghelper.ChangeDB(table.Database);
                        currentDbName = table.Database;
                    }

                    Console.WriteLine(new string('=', EQUALS_CHAR_COUNT));

                    if (
                        ExecAndSaveTable(
                            pghelper: pghelper,
                            table: table,
                            configJson: configJson,
                            onNullMessage: ConstMessages.FETCH_TBL_NOT_EXIST.Format(
                                args: [table.Database, table.Schema, table.Table]
                            ),
                            type: ScriptDataModel.ScriptType.Table,
                            query: ConstQueries.FETCH_TABLES.BindWith(
                                new Dictionary<string, object>
                                {
                                    { "{{schema_name}}", table.Schema },
                                    { "{{table_name}}", table.Table }
                                }
                            )
                        )
                    )
                    {
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

                        if (table.FetchTriggers == true)
                            ExecAndSaveTableProperties(
                                pghelper: pghelper,
                                table: table,
                                configJson: configJson,
                                type: ScriptDataModel.ScriptType.Trigger,
                                query: ConstQueries.FETCH_TABLES_TRIGGERS.BindWith(
                                    new Dictionary<string, object>
                                    {
                                        { "{{schema_name}}", table.Schema },
                                        { "{{table_name}}", table.Table }
                                    }
                                )
                            );
                    }

                    ConstMessages.FETCH_TBL_COMPLETED.WriteLine(
                        args: [table.Database, table.Schema, table.Table]
                    );
                }
            );

        ConstMessages.FETCH_TBL_END.WriteLine();
    }

    static void DDLFetchFunctions(PgSqlHelper pghelper, ConfigurationModel configJson)
    {
        ConstMessages.FETCH_FUNC_START.WriteLine();

        configJson
            .FetchDDL?.Functions?.ToList()
            .ForEach(
                (FunctionModel function) =>
                {
                    if (currentDbName != function.Database)
                    {
                        ConstMessages.CONN_DB_SWITCH.WriteLine(args: function.Database);

                        pghelper.ChangeDB(function.Database);
                        currentDbName = function.Database;
                    }

                    Console.WriteLine(new string('=', EQUALS_CHAR_COUNT));

                    ExecAndSaveFunction(
                        pghelper: pghelper,
                        function: function,
                        configJson: configJson,
                        query: ConstQueries.FETCH_FUNCTIONS.BindWith(
                            new Dictionary<string, object>
                            {
                                { "{{schema_name}}", function.Schema },
                                { "{{function_name}}", function.Function }
                            }
                        )
                    );

                    ConstMessages.FETCH_FUNC_COMPLETED.WriteLine(args: function.Function);
                }
            );

        ConstMessages.FETCH_FUNC_END.WriteLine();
    }

    private static bool ExecAndSaveTable(
        PgSqlHelper pghelper,
        TableModel table,
        ConfigurationModel configJson,
        ScriptDataModel.ScriptType type,
        string onNullMessage,
        string query
    )
    {
        ConstMessages.FETCH_TBL_GET_INFO.WriteLine(
            args: [table.Database, table.Schema, table.Table]
        );

        pghelper.ExecuteScalar(query, out string? result);

        if (string.IsNullOrWhiteSpace(result))
        {
            Console.WriteLine(onNullMessage);
            return false;
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

        return true;
    }

    private static void ExecAndSaveTableProperties(
        PgSqlHelper pghelper,
        TableModel table,
        ConfigurationModel configJson,
        ScriptDataModel.ScriptType type,
        string query
    )
    {
        pghelper.ExecuteQuery(query, out List<dynamic>? result);

        if (result is null)
        {
            ConstMessages.FETCH_TBL_PROP_NOT_FOUND.WriteLine(
                args: Enum.GetName(type) ?? string.Empty
            );
            return;
        }

        ConstMessages.FETCH_TBL_PROP_FOUND_COUNT.WriteLine(
            args: [Enum.GetName(type) ?? string.Empty, result.Count]
        );

        result.ForEach(x =>
        {
            dynamic jo = JObject.FromObject(x);
            ConstMessages.FETCH_TBL_PROP_PROC.WriteLine(
                args: [jo["name"].ToString(), Enum.GetName(type) ?? string.Empty]
            );

            dynamic script = jo["script"].ToString();
            WriteScript(
                script: script,
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

            List<string> functions = ExtractFunctions(script);
            ConstMessages.FETCH_TBL_PROP_FUNC_FOUND_COUNT.WriteLine(
                args:
                [
                    functions.Count,
                    Enum.GetName(type) ?? string.Empty,
                    jo["name"].ToString(),
                    string.Join(", ", functions)
                ]
            );

            if (functions.Count > 0)
            {
                functions.ForEach(x =>
                {
                    string[] xSplit = x.Split('.');
                    FunctionModel fm = new FunctionModel
                    {
                        Database = xSplit.Length > 2 ? xSplit[0] : table.Database,
                        Schema = xSplit.Length > 2 ? xSplit[1] : xSplit[0],
                        Function = xSplit.Length > 2 ? xSplit[2] : xSplit[1]
                    };

                    ConstMessages.FETCH_TBL_PROP_FUNC_PROC.WriteLine(args: [new string(' ', 9), x]);
                    ExecAndSaveFunction(
                        pghelper: pghelper,
                        function: fm,
                        configJson: configJson,
                        query: ConstQueries.FETCH_FUNCTIONS.BindWith(
                            new Dictionary<string, object>
                            {
                                { "{{schema_name}}", fm.Schema },
                                { "{{function_name}}", fm.Function }
                            }
                        ),
                        indentMessage: 9
                    );
                });
            }
        });
    }

    private static void ExecAndSaveFunction(
        PgSqlHelper pghelper,
        FunctionModel function,
        ConfigurationModel configJson,
        string query,
        int indentMessage = 0
    )
    {
        pghelper.ExecuteQuery(query, out List<dynamic>? result);

        if (result is null)
        {
            ConstMessages.FETCH_FUNC_NOT_EXIST.WriteLine(
                args: [new string(' ', indentMessage), function.Function]
            );
            return;
        }

        ConstMessages.FETCH_FUNC_PROC_FOUND_COUNT.WriteLine(
            args: [new string(' ', indentMessage), function.Function, result.Count]
        );

        result.ForEach(x =>
        {
            dynamic jo = JObject.FromObject(x);
            ConstMessages.FETCH_FUNC_PROC_START.WriteLine(
                args: [new string(' ', indentMessage), jo["name"].ToString()]
            );

            dynamic script = jo["script"].ToString();
            WriteScript(
                script: script,
                filename: Path.Combine(
                    path1: configJson.OutputDirectory,
                    path2: function.Database,
                    path3: function.Schema,
                    path4: FilenameBuilder(
                        new ScriptDataModel
                        {
                            Type = ScriptDataModel.ScriptType.Function,
                            ConfigJson = configJson,
                            ObjectName = jo["name"].ToString(),
                            ParameterCount = int.Parse(jo["pcount"].ToString() ?? "0"),
                            Schema = function.Schema,
                            Database = function.Database
                        }
                    )
                )
            );

            if (function.IterateInnerFunctions == false)
                return;

            List<string> functions = ExtractFunctions(script);
            ConstMessages.FETCH_FUNC_INSIDE_FUNC_COUNT.WriteLine(
                args:
                [
                    new string(' ', indentMessage + 2),
                    functions.Count,
                    jo["name"].ToString(),
                    string.Join(", ", functions)
                ]
            );

            if (functions.Count > 0)
            {
                functions.ForEach(x =>
                {
                    string[] xSplit = x.Split('.');
                    if (xSplit.Length < 2)
                    {
                        ConstMessages.FETCH_FUNC_INSIDE_FUNC_SKIP.WriteLine(
                            args: [new string(' ', indentMessage + 4), x]
                        );
                        return;
                    }

                    FunctionModel fm = new FunctionModel
                    {
                        Database = xSplit.Length > 2 ? xSplit[0] : function.Database,
                        Schema = xSplit.Length > 2 ? xSplit[1] : xSplit[0],
                        Function = xSplit.Length > 2 ? xSplit[2] : xSplit[1],
                        IterateInnerFunctions = function.IterateInnerFunctions
                    };

                    ConstMessages.FETCH_FUNC_INSIDE_FUNC_PROC.WriteLine(
                        args: [new string(' ', indentMessage + 4), x]
                    );
                    ExecAndSaveFunction(
                        pghelper: pghelper,
                        function: fm,
                        configJson: configJson,
                        query: ConstQueries.FETCH_FUNCTIONS.BindWith(
                            new Dictionary<string, object>
                            {
                                { "{{schema_name}}", fm.Schema },
                                { "{{function_name}}", fm.Function }
                            }
                        ),
                        indentMessage: indentMessage + 4
                    );
                });
            }
        });
    }

    [GeneratedRegex(
        pattern: @"(?i)\b(CALL|SELECT|PERFORM|EXECUTE FUNCTION|FROM)\s+([\w\$\.]+)\s*\("
    )]
    private static partial Regex FunctionRegex();

    static List<string> ExtractFunctions(string query)
    {
        MatchCollection matches = FunctionRegex().Matches(query);

        HashSet<string> functionNames = new HashSet<string>();

        foreach (Match match in matches)
        {
            if (match.Groups[2].Success)
                functionNames.Add(match.Groups[2].Value);
        }

        return functionNames.ToList();
    }

    static ConfigurationModel? ScriptPreparation()
    {
        restart_process:

        try
        {
            ConstMessages.SCR_PREP_ASK_PATH.WriteLine();

            string? userResponse = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userResponse))
                throw new ArgumentException(ConstMessages.SCR_PREP_PATH_CANNOT_EMPTY);

            if (File.Exists(userResponse) == false)
                throw new FileNotFoundException(ConstMessages.SCR_PREP_FILE_NOT_FOUND);

            ConstMessages.SCR_PREP_READ_CFG_FILE.WriteLine();
            string configFileContent = File.ReadAllText(userResponse!);
            ConfigurationModel configJson =
                JsonConvert.DeserializeObject<ConfigurationModel>(configFileContent)
                ?? throw new DataException(ConstMessages.SCR_PREP_READ_CFG_FAIL);

            return configJson;
        }
        catch (Exception ex)
        {
            ConstMessages.CMD_MSG_ERROR.WriteLine(
                args: [ex.Message, ex.StackTrace ?? string.Empty]
            );
            ConstMessages.CMD_MSG_RETRY_OR_MAIN_MENU.WriteLine();
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
            ConstMessages.CMD_MSG_ERROR.WriteLine(
                args: [ex.Message, ex.StackTrace ?? string.Empty]
            );
            return default;
        }
    }
}
