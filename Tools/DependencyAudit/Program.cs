using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace DependencyAudit;

internal static class Program
{
    private static readonly HashSet<string> Forbidden = new(StringComparer.OrdinalIgnoreCase)
    {
        "System.Windows.Forms", "System.Windows.Forms.Primitives", "WindowsFormsIntegration",
        "TombLib.Forms", "DarkUI", "NAudio.WinForms"
    };

    private static readonly HashSet<string> VendorExceptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "SharpDX", "SharpDX.Toolkit.Graphics"
    };

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: DependencyAudit <application.dll> [application.dll ...]");
            return 2;
        }

        try
        {
            bool succeeded = true;
            foreach (var application in args)
                succeeded &= Audit(Path.GetFullPath(application));

            return succeeded ? 0 : 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }
    }

    private static bool Audit(string application)
    {
        var directory = Path.GetDirectoryName(application) ?? throw new ArgumentException("An application directory is required.");
        var depsPath = Path.ChangeExtension(application, ".deps.json");
        using var deps = JsonDocument.Parse(File.ReadAllText(depsPath));
        var errors = new List<string>();
        var exceptions = new HashSet<string>(StringComparer.Ordinal);
        CheckFrameworks(application, errors);
        var assets = ReadAssets(deps.RootElement, directory, errors);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<string>(assets.Values);
        pending.Enqueue(application);

        while (pending.TryDequeue(out var path))
        {
            if (!visited.Add(path))
                continue;

            using var stream = File.OpenRead(path);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata)
                continue;

            var metadata = pe.GetMetadataReader();
            var source = metadata.GetString(metadata.GetAssemblyDefinition().Name);
            foreach (var handle in metadata.AssemblyReferences)
            {
                var reference = metadata.GetString(metadata.GetAssemblyReference(handle).Name);
                if (Forbidden.Contains(reference))
                {
                    var finding = $"{source} -> {reference}";
                    if (VendorExceptions.Contains(source) && reference.StartsWith("System.Windows.Forms", StringComparison.Ordinal))
                        exceptions.Add(finding);
                    else
                        errors.Add(finding);
                }

                if (assets.TryGetValue(reference, out var dependency))
                    pending.Enqueue(dependency);
            }
        }

        Console.WriteLine($"{Path.GetFileName(application)}: {visited.Count} local assemblies inspected; {errors.Count} forbidden dependencies.");
        foreach (var finding in exceptions.OrderBy(value => value, StringComparer.Ordinal))
            Console.WriteLine($"  Vendor exception retained: {finding}");
        foreach (var error in errors.Distinct(StringComparer.Ordinal))
            Console.Error.WriteLine($"  ERROR: {error}");

        return errors.Count == 0;
    }

    private static Dictionary<string, string> ReadAssets(JsonElement root, string directory, List<string> errors)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var targetName = root.GetProperty("runtimeTarget").GetProperty("name").GetString()
            ?? throw new InvalidDataException("The dependency manifest has no runtime target.");
        var target = root.GetProperty("targets").GetProperty(targetName);
        foreach (var library in target.EnumerateObject())
        {
            var name = library.Name.Split('/')[0];
            if (Forbidden.Contains(name))
                errors.Add($"Dependency manifest includes {name}.");

            foreach (var assetGroup in new[] { "runtime", "runtimeTargets" })
            {
                if (!library.Value.TryGetProperty(assetGroup, out var runtime))
                    continue;

                foreach (var asset in runtime.EnumerateObject())
                {
                    if (!asset.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (assetGroup == "runtimeTargets" && asset.Value.TryGetProperty("assetType", out var assetType) && assetType.GetString() != "runtime")
                        continue;

                    var fileName = Path.GetFileName(asset.Name);
                    var relativePath = Path.Combine(directory, asset.Name.Replace('/', Path.DirectorySeparatorChar));
                    var path = File.Exists(relativePath) ? relativePath : Path.Combine(directory, fileName);
                    if (File.Exists(path))
                        result.TryAdd(Path.GetFileNameWithoutExtension(fileName), path);
                }
            }
        }

        return result;
    }

    private static void CheckFrameworks(string application, List<string> errors)
    {
        using var configuration = JsonDocument.Parse(File.ReadAllText(Path.ChangeExtension(application, ".runtimeconfig.json")));
        var options = configuration.RootElement.GetProperty("runtimeOptions");
        if (options.TryGetProperty("framework", out var framework))
            CheckFramework(framework, errors);

        foreach (var property in new[] { "frameworks", "includedFrameworks" })
        {
            if (!options.TryGetProperty(property, out var frameworks))
                continue;

            foreach (var entry in frameworks.EnumerateArray())
                CheckFramework(entry, errors);
        }
    }

    private static void CheckFramework(JsonElement framework, List<string> errors)
    {
        if (framework.GetProperty("name").GetString() is "Microsoft.WindowsDesktop.App.WindowsForms")
            errors.Add("Runtime configuration requires the Windows Forms framework.");
    }
}
