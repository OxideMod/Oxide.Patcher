using System.Collections.Generic;
using Mono.Cecil;
using Oxide.Patcher.Common;

namespace Oxide.Patcher.Docs
{
    public class DocsMethodData
    {
        public string MethodName { get; set; }
        public string ReturnType { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();

        public DocsMethodData(MethodDefinition methodDef)
        {
            MethodName = methodDef.Name;
            ReturnType = Utility.TransformType(methodDef.ReturnType.FullName);

            foreach (ParameterDefinition parameterDef in methodDef.Parameters)
            {
                Arguments[parameterDef.Name] = Utility.TransformType(parameterDef.ParameterType.FullName);
            }
        }
    }
}
