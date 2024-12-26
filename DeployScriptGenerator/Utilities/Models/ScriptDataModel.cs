using DeployScriptGenerator.Utilities.Models;

namespace DeployScriptGenerator.Utilities.Models;

internal partial class Program
{
    internal class ScriptDataModel
    {
        internal enum ScriptType
        {
            Table,
            Constraint,
            Index,
            Trigger,
            Function,
            View
        }

        internal required ScriptType Type { get; set; }

        internal string OperationType() =>
            Type switch
            {
                ScriptType.Table => "CRTB",
                ScriptType.Constraint => "ALTTBL",
                ScriptType.Index => "CRIDX",
                ScriptType.Trigger => "CRTGR",
                ScriptType.Function => "CRFUN",
                ScriptType.View => "CRTBLVW",
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
            };

        internal required string Database { get; set; }
        internal required string Schema { get; set; }
        internal required string ObjectName { get; set; }

        internal int ParameterCount { get; set; } = 0;

        internal required ConfigurationModel ConfigJson { get; set; }
    }
}
