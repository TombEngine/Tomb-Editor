using System;
using System.Linq;

namespace TombIDE.Shared.NewStructure
{
	/// <summary>
	/// Helper class for parsing TRX engine version strings.
	/// </summary>
	public static class TRXVersionHelper
	{
		/// <summary>
		/// Version prefix used in modern TRX engine executables.
		/// </summary>
		public const string ModernVersionPrefix = "TRX ";

		/// <summary>
		/// Parses a version string from a TRX-based engine, handling both modern and legacy version formats.
		/// </summary>
		/// <param name="versionString">The version string to parse.</param>
		/// <param name="legacyVersionPrefix">The prefix used by legacy versions (e.g., "TR1X ", "TR2X ", or empty for no legacy prefix).</param>
		/// <param name="hasLegacyPrefix">Whether the engine uses a legacy prefix for old versions.</param>
		/// <returns>A parsed Version object with the appropriate major version.</returns>
		/// <exception cref="FormatException">Thrown when the version string is empty after removing the prefix.</exception>
		public static Version ParseTRXVersion(string versionString, string legacyVersionPrefix, bool hasLegacyPrefix = true)
		{
			bool isLegacyVersion = DetermineLegacyVersion(versionString, legacyVersionPrefix, hasLegacyPrefix);
			string versionPrefix = isLegacyVersion && hasLegacyPrefix ? legacyVersionPrefix : ModernVersionPrefix;

			// Remove the appropriate prefix
			if (isLegacyVersion && hasLegacyPrefix)
				versionString = versionString.Replace(legacyVersionPrefix, string.Empty);
			else if (!isLegacyVersion)
				versionString = versionString.Replace(ModernVersionPrefix, string.Empty);

			versionString = versionString.Trim();

			if (string.IsNullOrEmpty(versionString))
				throw new FormatException("Version string is empty after removing prefix.");

			// For legacy versions, always prepend "0." to indicate it's a legacy version
			if (isLegacyVersion)
			{
				// Count version components (dots + 1)
				int componentCount = versionString.Count(c => c == '.') + 1;

				// If the version has 4 components, drop the revision (last component) to make room for the "0." prefix
				// This ensures the "0." prefix is always present for legacy versions
				if (componentCount >= 4)
				{
					// Split and take only the first 3 components
					string[] parts = versionString.Split('.');
					versionString = string.Join(".", parts.Take(3));
				}

				return new Version("0." + versionString); // Legacy versions get a 0.x major version
			}

			return new Version(versionString);
		}

		/// <summary>
		/// Determines whether a version string represents a legacy version.
		/// </summary>
		private static bool DetermineLegacyVersion(string versionString, string legacyVersionPrefix, bool hasLegacyPrefix)
		{
			if (!hasLegacyPrefix)
			{
				// If there's no legacy prefix support, check if it doesn't start with modern prefix
				return !versionString.StartsWith(ModernVersionPrefix);
			}

			// If legacy prefix is defined, check if version starts with it
			return versionString.StartsWith(legacyVersionPrefix);
		}
	}
}
