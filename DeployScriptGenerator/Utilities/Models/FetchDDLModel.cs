using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class FetchDDLModel
{
    [JsonProperty("tables")]
    public TableModel[]? Tables { get; set; }

    [JsonProperty("functions")]
    public FunctionModel[]? Functions { get; set; }

    [JsonProperty("views")]
    public ViewModel[]? Views { get; set; }
}
