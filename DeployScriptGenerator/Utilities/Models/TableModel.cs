using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class TableModel : SchemaModel
{
    [
        JsonProperty("table"),
        Required(ErrorMessage = "TableModel: table is required.", AllowEmptyStrings = false)
    ]
    public required string Table { get; set; }

    [JsonProperty("fetch_constraints", NullValueHandling = NullValueHandling.Ignore)]
    public bool? FetchConstraints { get; set; } = true;

    [JsonProperty("fetch_indexes", NullValueHandling = NullValueHandling.Ignore)]
    public bool? FetchIndexes { get; set; } = true;

    [JsonProperty("fetch_triggers", NullValueHandling = NullValueHandling.Ignore)]
    public bool? FetchTriggers { get; set; } = true;

    [JsonProperty("fetch_functions", NullValueHandling = NullValueHandling.Ignore)]
    public bool? FetchFunctions { get; set; } = true;
}
