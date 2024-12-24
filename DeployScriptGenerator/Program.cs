using DeployScriptGenerator.Utilities.Constants;
using DeployScriptGenerator.Utilities.Models;
using Newtonsoft.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        ShowWelcomeMessage();
    }

    static void ShowWelcomeMessage()
    {
        Console.WriteLine(ConstMessages.CMD_MSG_WELCOME);

        var userResponse = Console.ReadLine();

        switch (userResponse)
        {
            case "1":
                GenerateSampleJsonConfigs();
                break;
            case "2":
                GenerateSampleJsonConfigs();
                break;
            case "3":
                Console.WriteLine(ConstMessages.CMD_MSG_EXITING);
                return;
            default:
                InvalidResponse(userResponse);
                break;
        }
    }

    static void InvalidResponse(string? userResponse)
    {
        Console.Clear();
        Console.WriteLine(ConstMessages.CMD_MSG_INVALID_RESPONSE, userResponse);
        ShowWelcomeMessage();
    }

    static void GenerateSampleJsonConfigs()
    {
        Console.Clear();
        Console.WriteLine("Please specify the path of the sample json configuration file.");
        var userResponse = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userResponse))
        {
            GenerateSampleJsonConfigs();
            return;
        }

        if (Directory.Exists(Path.GetDirectoryName(userResponse)) == false)
        {
            Console.WriteLine("Directory does not exist. Please try again.");
            GenerateSampleJsonConfigs();
            return;
        }

        File.WriteAllText(
            path: userResponse,
            contents: (string?)
                JsonConvert.SerializeObject(
                    formatting: Formatting.Indented,
                    value: new ConfigurationModel
                    {
                        OutputDirectory =
                            "/media/fadhly/Data/-Repo/deployment_scripts/DeployScriptGenerator/Data/sampah/sample_output",
                        TicketNumber = "T123456",
                        FetchDDL = new FetchDDLModel
                        {
                            Tables =
                            [
                                new TableModel
                                {
                                    Db = "idc.en",
                                    Schema = "public",
                                    Table = "component_de_cb"
                                },
                                new TableModel
                                {
                                    Db = "idc.en",
                                    Schema = "in_memory",
                                    Table = "df_sync_status"
                                }
                            ],
                            Functions =
                            [
                                new FunctionModel
                                {
                                    Db = "idc.en",
                                    Schema = "in_memory",
                                    Function = "component_de_select"
                                },
                                new FunctionModel
                                {
                                    Db = "idc.en",
                                    Schema = "in_memory",
                                    Function = "config_master_param_generator"
                                }
                            ],
                            Views =
                            [
                                new ViewModel
                                {
                                    Db = "idc.en",
                                    Schema = "public",
                                    View = "vw_los_data_mapping"
                                },
                                new ViewModel
                                {
                                    Db = "idc.en",
                                    Schema = "public",
                                    View = "vw_master_summary"
                                }
                            ]
                        },
                        CreateDDL = new CreateDDLModel
                        {
                            Databases = ["idc.en", "idc.kaml"],
                            Schemas = [new SchemaModel { Db = "idc.en", Schema = "in_memory" }]
                        }
                    }
                )
        );
        Console.Clear();
        Console.WriteLine($"Sample configuration json file successfully saved to: {userResponse}");
    }
}
