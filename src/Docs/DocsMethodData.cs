using System.Collections.Generic;
using Mono.Cecil;

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
                Arguments[Utility.TransformType(parameterDef.ParameterType.FullName)] = parameterDef.Name;
            }
        }
    }
}
