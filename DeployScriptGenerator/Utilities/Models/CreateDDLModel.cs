using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class CreateDDLModel
{
    [JsonProperty("databases")]
    public string[]? Databases { get; set; }

    [JsonProperty("schemas")]
    public SchemaModel[]? Schemas { get; set; }
}
