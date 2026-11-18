#nullable enable

using System;
using System.Collections.Generic;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Finds event set tree rows by name for the search popup.</summary>
    public static class SetTreeSearch
    {
        /// <summary>
        /// Returns the first row whose name contains <paramref name="text"/>, searching from the current row
        /// (or after it when <paramref name="skipCurrent"/> is set) and wrapping around the end of the list.
        /// </summary>
        public static SetTreeNode? FindNext(IList<SetTreeNode> orderedNodes, SetTreeNode? current, string text, bool skipCurrent)
        {
            if (text.Length == 0 || orderedNodes.Count == 0)
                return null;

            int start = current is null ? 0 : Math.Max(orderedNodes.IndexOf(current), 0);
            if (skipCurrent && current is not null)
                start++;

            for (int i = 0; i < orderedNodes.Count; i++)
            {
                var node = orderedNodes[(start + i) % orderedNodes.Count];
                if (node.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                    return node;
            }

            return null;
        }
    }
}
