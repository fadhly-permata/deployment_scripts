using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class FunctionModel : SchemaModel
{
    [
        JsonProperty("function"),
        Required(ErrorMessage = "FunctionModel: function is required.", AllowEmptyStrings = false)
    ]
    public required string Function { get; set; }
}
