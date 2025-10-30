using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;

namespace Hammer.Extensions;

/// <summary>
///     Represents a builder for constructing custom IDs with key-value pairs.
/// </summary>
public struct CustomIdBuilder
{
    private readonly Dictionary<string, string> _pairs = new();
    private string? _type = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomIdBuilder" /> class.
    /// </summary>
    public CustomIdBuilder()
    {
    }

    /// <summary>
    ///     Sets the type of the custom ID.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The current <see cref="CustomIdBuilder" />.</returns>
    public CustomIdBuilder Type(string type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    ///     Adds a key-value pair to the custom ID.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>The current <see cref="CustomIdBuilder" />.</returns>
    public CustomIdBuilder Add<T>(string key, T value)
    {
        _pairs[key] = value?.ToString()!;
        return this;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        // e.g. namespace:id;name=Foo;scope=guild
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
        builder.Append(_type ?? string.Empty);

        foreach ((string key, string value) in _pairs)
        {
            builder.Append(';');
            builder.Append(key);
            builder.Append('=');
            builder.Append(value);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Tries to parse a custom ID into its type and key-value pairs.
    /// </summary>
    /// <param name="input">The input to parse.</param>
    /// <param name="type">
    ///     When this method returns, contains the type if the parsing succeeded, or <see langword="null" /> if it failed.
    /// </param>
    /// <param name="pairs">
    ///     When this method returns, contains the key-value pairs if the parsing succeeded, or an empty dictionary if it
    ///     failed.
    /// </param>
    /// <returns><see langword="true" /> if the parsing succeeded; otherwise, <see langword="false" />.</returns>
    public static bool TryParse(
        ReadOnlySpan<char> input,
        [NotNullWhen(true)] out string? type,
        [NotNullWhen(true)] out IReadOnlyDictionary<string, string>? pairs)
    {
        var dictionary = new Dictionary<string, string>();
        int separatorIndex = input.IndexOf(';');

        if (separatorIndex < 0)
        {
            type = input.ToString();
            pairs = dictionary;
            return true;
        }

        type = input[..separatorIndex].ToString();
        input = input[(separatorIndex + 1)..];

        while (!input.IsEmpty)
        {
            int eq = input.IndexOf('=');
            if (eq < 0)
            {
                break;
            }

            var key = input[..eq].ToString();
            input = input[(eq + 1)..];

            int end = input.IndexOf(';');
            ReadOnlySpan<char> value;

            if (end < 0)
            {
                value = input;
                input = ReadOnlySpan<char>.Empty;
            }
            else
            {
                value = input[..end];
                input = input[(end + 1)..];
            }

            dictionary[key] = value.ToString();
        }

        pairs = dictionary;
        return true;
    }
}
