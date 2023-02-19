namespace Oxide.Patcher
{
    public enum MethodExposure
    {
        Private,
        Protected,
        Public,
        Internal
    }

    public enum Exposure
    {
        Private,
        Protected,
        Public,
        Internal,
        Static,
        Null
    }

    public enum ModifierType
    {
        Field,
        Method,
        Property,
        Type
    }
}
