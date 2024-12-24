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
}
