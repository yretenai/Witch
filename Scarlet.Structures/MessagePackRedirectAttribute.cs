namespace Scarlet.Structures;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class MessagePackRedirectAttribute : Attribute {
    public virtual Type Target => null!;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MessagePackRedirectAttribute<T> : MessagePackRedirectAttribute {
    public override Type Target { get; } = typeof(T);
}
