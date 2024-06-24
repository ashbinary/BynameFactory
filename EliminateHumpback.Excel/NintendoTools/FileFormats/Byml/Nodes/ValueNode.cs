using System;

namespace NintendoTools.FileFormats.Byml;

/// <summary>
/// The base class for a value-type node.
/// </summary>
public abstract class ValueNode : Node
{
    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// Gets the value of the node.
    /// </summary>
    public abstract object? GetValue();
}

/// <summary>
/// A class for a value-type node.
/// </summary>
public class ValueNode<T> : ValueNode
{
    /// <inheritdoc />
    public override Type ValueType => typeof(T);

    /// <summary>
    /// Gets or sets the value of the node.
    /// </summary>
    public T? Value { get; set; }

    /// <inheritdoc />
    public override object? GetValue() => Value;
}