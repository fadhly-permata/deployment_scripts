using DeployScriptGenerator.Utilities.Models;
using Newtonsoft.Json;

namespace DeployScriptGenerator;

internal partial class Program
{
    static void GenerateSampleJsonConfigs()
    {
        restart_process:

        Console.WriteLine("Please specify the path of the sample json configuration file.");
        var userResponse = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userResponse))
        {
            Console.WriteLine("Path can not be empty.");
            Console.WriteLine(
                "Press <r> to try again or press any other key to go back to the main menu?"
            );
            if (Console.ReadLine()?.ToLower() == "r")
                goto restart_process;
            else
                ShowWelcomeMessage();
        }

        if (Directory.Exists(Path.GetDirectoryName(userResponse)) == false)
        {
            Console.WriteLine("Directory does not exist. Please try again.");
            Console.WriteLine(
                "Press <r> to try again or press any other key to go back to the main menu?"
            );
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
                            "Server=127.0.0.1;Port=5432;Database=idc.en;User Id=postgres;Password=postgres;",
                        OutputDirectory =
                            "/media/fadhly/Data/-Repo/deployment_scripts/DeployScriptGenerator/Data/sampah/sample_output",
                        TicketNumber = "T123456",
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
        Console.WriteLine($"Sample configuration json file successfully saved to: {userResponse}");
        Console.WriteLine("Press [Enter] key to go back to the main menu.");
        Console.Read();
        ShowWelcomeMessage();
    }
}
