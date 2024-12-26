using DeployScriptGenerator.Utilities.Constants;
using DeployScriptGenerator.Utilities.Extensions.Strings;
using DeployScriptGenerator.Utilities.Models;
using Newtonsoft.Json;

namespace DeployScriptGenerator;

internal partial class Program
{
    static void GenerateSampleJsonConfigs()
    {
        restart_process:

        ConstMessages.SMPL_CFG_ASK_PATH.WriteLine();
        string? userResponse = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userResponse))
        {
            ConstMessages.SMPL_CFG_PATH_CANNOT_EMPTY.WriteLine();
            ConstMessages.CMD_MSG_RETRY_OR_MAIN_MENU.WriteLine();

            if (Console.ReadLine()?.ToLower() == "r")
                goto restart_process;
            else
                ShowWelcomeMessage();
        }

        if (Directory.Exists(Path.GetDirectoryName(userResponse)) == false)
        {
            ConstMessages.SMPL_CFG_DIR_NOT_FOUND.WriteLine();
            ConstMessages.CMD_MSG_RETRY_OR_MAIN_MENU.WriteLine();

            if (Console.ReadLine()?.ToLower() == "r")
                goto restart_process;
            else
                ShowWelcomeMessage();
        }

        File.WriteAllText(
            path: userResponse!,
            contents: (string?)
                JsonConvert.SerializeObject(
                    formatting: Formatting.Indented,
                    value: new ConfigurationModel
                    {
                        ConnectionString =
                            "User ID=idc_fadhly;HOST=localhost;Port=5432;Database=idc.kaml;Integrated Security=true;Pooling=true;MinPoolSize=1;MaxPoolSize=1000;",
                        OutputDirectory =
                            "/media/fadhly/Data/-Repo/deployment_scripts/DeployScriptGenerator/Data/sampah/sample_output",
                        TicketNumber = "T123456",
                        CleanupDirectoryFirst = true,
                        FetchDDL = new FetchDDLModel
                        {
                            Tables =
                            [
                                new TableModel
                                {
                                    Database = "idc.en",
                                    Schema = "public",
                                    Table = "component_de_cb"
                                },
                                new TableModel
                                {
                                    Database = "idc.en",
                                    Schema = "in_memory",
                                    Table = "df_sync_status"
                                }
                            ],
                            Functions =
                            [
                                new FunctionModel
                                {
                                    Database = "idc.en",
                                    Schema = "in_memory",
                                    Function = "component_de_select"
                                },
                                new FunctionModel
                                {
                                    Database = "idc.en",
                                    Schema = "in_memory",
                                    Function = "config_master_param_generator"
                                }
                            ],
                            Views =
                            [
                                new ViewModel
                                {
                                    Database = "idc.en",
                                    Schema = "public",
                                    View = "vw_los_data_mapping"
                                },
                                new ViewModel
                                {
                                    Database = "idc.en",
                                    Schema = "public",
                                    View = "vw_master_summary"
                                }
                            ]
                        }
                    }
                )
        );
        Console.Clear();
        ConstMessages.SMPL_CFG_SAVED.WriteLine(args: userResponse!);
        ConstMessages.CMD_MSG_RESTART_APP.WriteLine();
        Console.Read();
        ShowWelcomeMessage();
    }
}
