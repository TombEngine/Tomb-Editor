using System.Collections.Generic;
using TombIDE.ProjectMaster.Services.FileExtraction;
using TombIDE.ProjectMaster.Services.Plugins.Deployment;
using TombIDE.ProjectMaster.Services.Plugins.Discovery;
using TombIDE.ProjectMaster.Services.Plugins.Initialization;
using TombIDE.ProjectMaster.Services.Plugins.Installation;
using TombIDE.ProjectMaster.Services.Plugins.Metadata;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.Plugins;

/// <summary>
/// Factory for creating plugin services based on the game version.
/// </summary>
public interface IPluginServiceFactory
{
	/// <summary>
	/// Gets the plugin discovery service for the specified game version.
	/// </summary>
	IPluginDiscoveryService? GetDiscoveryService(TRVersion.Game gameVersion);

	/// <summary>
	/// Gets the plugin installation service for the specified game version.
	/// </summary>
	IPluginInstallationService? GetInstallationService(TRVersion.Game gameVersion);

	/// <summary>
	/// Gets the plugin deployment service for the specified game version.
	/// </summary>
	IPluginDeploymentService? GetDeploymentService(TRVersion.Game gameVersion);

	/// <summary>
	/// Gets the plugin metadata service for the specified game version.
	/// </summary>
	IPluginMetadataService? GetMetadataService(TRVersion.Game gameVersion);

	/// <summary>
	/// Gets the plugin initialization service for the specified game version.
	/// </summary>
	IPluginInitializationService? GetInitializationService(TRVersion.Game gameVersion);
}

/// <summary>
/// Default implementation of the plugin service factory.
/// </summary>
public sealed class PluginServiceFactory : IPluginServiceFactory
{
	// Cache services per game version

	private readonly Dictionary<TRVersion.Game, IPluginDiscoveryService> _discoveryServices = [];
	private readonly Dictionary<TRVersion.Game, IPluginInstallationService> _installationServices = [];
	private readonly Dictionary<TRVersion.Game, IPluginDeploymentService> _deploymentServices = [];
	private readonly Dictionary<TRVersion.Game, IPluginMetadataService> _metadataServices = [];
	private readonly Dictionary<TRVersion.Game, IPluginInitializationService> _initializationServices = [];

	private readonly IFileExtractionService _fileExtractionService = new FileExtractionService();

	public IPluginDiscoveryService? GetDiscoveryService(TRVersion.Game gameVersion)
	{
		if (_discoveryServices.TryGetValue(gameVersion, out var service))
			return service;

		var metadataService = GetMetadataService(gameVersion);

		service = gameVersion switch
		{
			TRVersion.Game.TRNG when metadataService is not null => new TRNGPluginDiscoveryService(metadataService),
			// Add other game versions here in the future
			_ => null
		};

		if (service is not null)
			_discoveryServices[gameVersion] = service;

		return service;
	}

	public IPluginInstallationService? GetInstallationService(TRVersion.Game gameVersion)
	{
		if (_installationServices.TryGetValue(gameVersion, out var service))
			return service;

		var metadataService = GetMetadataService(gameVersion);

		service = gameVersion switch
		{
			TRVersion.Game.TRNG when metadataService is not null => new TRNGPluginInstallationService(metadataService, _fileExtractionService),
			// Add other game versions here in the future
			_ => null
		};

		if (service is not null)
			_installationServices[gameVersion] = service;

		return service;
	}

	public IPluginDeploymentService? GetDeploymentService(TRVersion.Game gameVersion)
	{
		if (_deploymentServices.TryGetValue(gameVersion, out var service))
			return service;

		service = gameVersion switch
		{
			TRVersion.Game.TRNG => new TRNGPluginDeploymentService(),
			// Add other game versions here in the future
			_ => null
		};

		if (service is not null)
			_deploymentServices[gameVersion] = service;

		return service;
	}

	public IPluginMetadataService? GetMetadataService(TRVersion.Game gameVersion)
	{
		if (_metadataServices.TryGetValue(gameVersion, out var service))
			return service;

		service = gameVersion switch
		{
			TRVersion.Game.TRNG => new TRNGPluginMetadataService(),
			// Add other game versions here in the future
			_ => null
		};

		if (service is not null)
			_metadataServices[gameVersion] = service;

		return service;
	}

	public IPluginInitializationService? GetInitializationService(TRVersion.Game gameVersion)
	{
		if (_initializationServices.TryGetValue(gameVersion, out var service))
			return service;

		service = gameVersion switch
		{
			TRVersion.Game.TRNG => new TRNGPluginInitializationService(),
			// Add other game versions here in the future
			_ => null
		};

		if (service is not null)
			_initializationServices[gameVersion] = service;

		return service;
	}
}
