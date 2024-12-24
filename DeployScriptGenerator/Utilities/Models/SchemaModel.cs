using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class SchemaModel
{
    [
        JsonProperty("db"),
        Required(
            ErrorMessage = "SchemaModel/FetchDDLModel/FunctionModel/TableModel/ViewModel: db is required.",
            AllowEmptyStrings = false
        )
    ]
    public required string Db { get; set; }

    [
        JsonProperty("schema"),
        Required(
            ErrorMessage = "SchemaModel/FetchDDLModel/FunctionModel/TableModel/ViewModel: schema is required.",
            AllowEmptyStrings = false
        )
    ]
    public required string Schema { get; set; }
}
