using Mono.Cecil;
using System.Collections.Generic;

namespace Oxide.Patcher.Patching.OxideDefinitions
{
    public class OxideTypeDefinition
    {
        private TypeDefinition type;
        private List<OxideTypeDefinition> genericTypeInstances;
        
        public OxideTypeDefinition(TypeDefinition type)
        {
            this.type = type;
            genericTypeInstances = new List<OxideTypeDefinition>();
        }

        public void AddGenericTypeInstance(OxideTypeDefinition genericTypeInstance)
        {
            genericTypeInstances.Add(genericTypeInstance);
        }

        public TypeReference GetTypeReference()
        {
            if (type.HasGenericParameters)
            {
                GenericInstanceType generic = new GenericInstanceType(type);
                foreach (OxideTypeDefinition genericType in genericTypeInstances)
                {
                    generic.GenericArguments.Add(genericType.GetTypeReference());
                }
                
                return generic;
            }
            
            return type;
        }
    }
}
