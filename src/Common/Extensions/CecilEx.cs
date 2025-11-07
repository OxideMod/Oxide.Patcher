using Mono.Cecil;

namespace Oxide.Patcher.Common.Extensions
{
    public static class CecilEx
    {
        // Copied from older version of ICSharpCode.Decompiler
        public static bool IsVoid(this TypeReference type)
        {
            while (true)
            {
                switch (type)
                {
                    case OptionalModifierType _:
                    case RequiredModifierType _:
                        type = ((TypeSpecification) type).ElementType;
                        continue;

                    default:
                        goto label_3;
                }
            }

            label_3:

            return type.MetadataType == MetadataType.Void;
        }
    }
}
