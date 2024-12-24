using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeployScriptGenerator.Utilities.Models;

internal class ViewModel : SchemaModel
{
    [
        JsonProperty("view"),
        Required(ErrorMessage = "ViewModel: view is required.", AllowEmptyStrings = false)
    ]
    public required string View { get; set; }
}
