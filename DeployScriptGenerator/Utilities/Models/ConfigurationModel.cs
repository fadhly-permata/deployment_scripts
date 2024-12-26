using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class ConfigurationModel
{
    [
        JsonProperty("connection_string"),
        Required(
            ErrorMessage = "ConfigurationModel: Connection String is required.",
            AllowEmptyStrings = false
        )
    ]
    public required string ConnectionString { get; set; }

    [
        JsonProperty("output_directory"),
        Required(
            ErrorMessage = "ConfigurationModel: Output Directory is required.",
            AllowEmptyStrings = false
        )
    ]
    public required string OutputDirectory { get; set; }

    [
        JsonProperty("ticket_number"),
        Required(
            ErrorMessage = "ConfigurationModel: Ticket Number is required.",
            AllowEmptyStrings = false
        ),
    ]
    public required string TicketNumber { get; set; }

    [JsonProperty("ddl_to_fetch")]
    public FetchDDLModel? FetchDDL { get; set; }
}
