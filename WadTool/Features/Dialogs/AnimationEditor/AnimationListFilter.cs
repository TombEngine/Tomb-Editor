#nullable enable

using System;
using TombLib.Wad;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>
/// Parses the animation list filter text (plain number, plain name or "token:value") and tests animations against it.
/// </summary>
public sealed class AnimationListFilter
{
    private enum FilterKind
    {
        None,
        StateId,
        AnimNumber,
        Name,
        StateName
    }

    private readonly FilterKind _kind;
    private readonly int _number;
    private readonly string _text;

    private AnimationListFilter(FilterKind kind, int number, string text)
    {
        _kind = kind;
        _number = number;
        _text = text;
    }

    /// <summary>
    /// Builds a filter from the user's text. The lookup resolves a (partial) state name to a state ID, or returns a negative value.
    /// </summary>
    public static AnimationListFilter Parse(string? input, Func<string, int> findStateByName)
    {
        var search = (input ?? string.Empty).Trim().ToLowerInvariant();
        var kind = FilterKind.None;
        var number = -1;
        var text = string.Empty;

        if (search.Length > 0)
        {
            var tokenEnd = search.IndexOf(':');

            if (tokenEnd >= 0)
            {
                var token = search[..tokenEnd];
                var value = search[(tokenEnd + 1)..];
                kind = GetTokenKind(token);

                // Force name search type if there is no number after a numerical token.
                var hasNumber = int.TryParse(value, out number);

                if (token.Length > 0 && (!hasNumber || kind == FilterKind.Name))
                {
                    text = value;
                    kind = kind == FilterKind.StateId ? FilterKind.StateName : FilterKind.Name;
                }
            }
            else if (!int.TryParse(search, out number))
            {
                kind = FilterKind.Name;
                text = search;
            }
            else
            {
                kind = FilterKind.StateId;
            }
        }

        // Known state names are resolved to state IDs, otherwise the filter does nothing.
        if (kind == FilterKind.StateName)
        {
            var stateId = findStateByName(text);

            if (stateId >= 0)
            {
                number = stateId;
                kind = FilterKind.StateId;
            }
        }

        return new AnimationListFilter(kind, number, text);
    }

    /// <summary>Returns whether the animation at the given list position passes the filter.</summary>
    public bool IsMatch(int animationIndex, WadAnimation animation) => _kind switch
    {
        FilterKind.StateId => _number < 0 || animation.StateId == _number,
        FilterKind.AnimNumber => _number < 0 || animationIndex == _number,
        FilterKind.Name => animation.Name.Contains(_text, StringComparison.OrdinalIgnoreCase),
        _ => true
    };

    private static FilterKind GetTokenKind(string token) => token switch
    {
        "num" or "n" or "number" or "anim" or "animation" or "a" => FilterKind.AnimNumber,
        "state" or "s" or "stateid" => FilterKind.StateId,
        "name" => FilterKind.Name,
        _ => FilterKind.None
    };
}
