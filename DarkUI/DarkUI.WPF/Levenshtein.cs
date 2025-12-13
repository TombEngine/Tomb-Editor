using System;

namespace DarkUI.WPF;

public static class Levenshtein
{
	public static int DistanceSubstring(string searched, string find, out int endIndex)
	{
		endIndex = find.Length;

		if (searched.Length == 0)
			return find.Length;

		if (find.Length == 0)
			return 0;

		// Create lookup array
		int[,] lookup = new int[searched.Length + 1, find.Length + 1];

		for (int i = 0; i <= searched.Length; ++i)
			lookup[i, 0] = i == 0 || char.IsWhiteSpace(searched[i - 1]) ? 0 : 1; // Immediate deletions

		for (int i = 0; i <= find.Length; ++i)
			lookup[0, i] = i; // Immediate insertions

		// Iterate
		for (int i = 1; i <= searched.Length; ++i)
			for (int j = 1; j <= find.Length; ++j)
			{
				lookup[i, j] = Math.Min(
					lookup[i - 1, j - 1] + (find[j - 1] == searched[i - 1] ? 0 : 1), //  Substitute
					Math.Min(
						lookup[i - 1, j] + 1, // Delete
						lookup[i, j - 1] + 1)); // Insert
			}

		// Find best match
		int result = int.MaxValue;

		for (int i = 0; i <= searched.Length; ++i)
		{
			if (lookup[i, find.Length] < result)
			{
				result = lookup[i, find.Length];
				endIndex = i; // Insertions at the end are without cost
			}
		}

		return result;
	}
}
