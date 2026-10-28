using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IconPipeline;

internal static class Program
{
    private static readonly string[] Buckets = ["@1x", "@2x", "@3x", "@4x"];
    private static readonly int[] Scales = [1, 2, 3, 4];
    private static readonly string[] Categories = ["Actions", "General", "Misc", "Objects", "Sectortype", "Texture", "Toolbox"];

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
            {
                PrintUsage();
                return 0;
            }

            var options = ParseOptions(args[1..]);
            var rootValue = options.TryGetValue("root", out var configuredRoot) ? configuredRoot : FindProjectRoot();
            var root = Path.GetFullPath(rootValue);
            var svgRoot = Path.Combine(root, "Svg");
            return args[0] switch
            {
                "inventory" => WriteInventory(root, svgRoot, options),
                "validate-svg-bounds" => ValidateSvgBounds(svgRoot),
                "validate-png" => ValidatePng(root, svgRoot, options),
                "rasterize" => Rasterize(root, svgRoot, options),
                "rasterize-legacy" => RasterizeLegacy(root, svgRoot, options),
                "contact-sheet" => WriteContactSheet(root, svgRoot, options),
                _ => throw new ArgumentException($"Unknown command '{args[0]}'.")
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static int WriteInventory(string root, string svgRoot, Dictionary<string, string> options)
    {
        var svgFiles = GetFiles(svgRoot, "*.svg");
        var legacyRoot = options.GetValueOrDefault("legacy-root");
        var fullLegacyRoot = legacyRoot is null ? null : Path.GetFullPath(legacyRoot);
        var legacyFiles = fullLegacyRoot is null ? [] : GetFiles(fullLegacyRoot, "*.png");
        var matchedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<string[]>();

        foreach (var legacyFile in legacyFiles)
        {
            var key = GetLegacyKey(legacyFile, svgFiles, svgRoot);
            var svgFile = svgFiles.FirstOrDefault(file => StringComparer.OrdinalIgnoreCase.Equals(GetSvgKey(file, svgRoot), key));
            var id = svgFile is null ? "" : GetSvgKey(svgFile, svgRoot);
            if (svgFile is not null)
                matchedIds.Add(id);
            rows.Add([Path.GetRelativePath(fullLegacyRoot ?? throw new InvalidOperationException(), legacyFile).Replace('\\', '/'), id, svgFile is null ? "legacy-only" : "matched"]);
        }

        foreach (var svgFile in svgFiles)
        {
            var id = GetSvgKey(svgFile, svgRoot);
            if (!matchedIds.Contains(id))
                rows.Add(["", id, legacyRoot is null ? "svg-only-legacy-not-provided" : "svg-only"]);
        }

        var output = Path.GetFullPath(options.GetValueOrDefault("output", Path.Combine(root, "icon-inventory.csv")));
        WriteCsv(output, ["legacy_png", "svg_id", "status"], rows);
        PrintCategoryMetrics(svgFiles, svgRoot);
        Console.WriteLine($"SVG total: {svgFiles.Count}");
        Console.WriteLine($"Legacy PNG total: {legacyFiles.Count}");
        Console.WriteLine($"Matched legacy PNGs: {rows.Count(row => row[2] == "matched")}");
        Console.WriteLine($"Legacy-only PNGs: {rows.Count(row => row[2] == "legacy-only")}");
        Console.WriteLine($"SVGs without a legacy match: {rows.Count(row => row[2].StartsWith("svg-only", StringComparison.Ordinal))}");
        Console.WriteLine($"Manifest: {output}");
        return 0;
    }

    private static int ValidatePng(string root, string svgRoot, Dictionary<string, string> options)
    {
        var pngRoot = Path.GetFullPath(options.GetValueOrDefault("png-root", Path.Combine(root, "Png")));
        var svgFiles = GetFiles(svgRoot, "*.svg");
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        for (var index = 0; index < Buckets.Length; index++)
        {
            foreach (var svgFile in svgFiles)
            {
                var relative = Path.ChangeExtension(Path.GetRelativePath(svgRoot, svgFile), ".png");
                var pngFile = Path.GetFullPath(Path.Combine(pngRoot, Buckets[index], relative));
                expected.Add(pngFile);
                if (!File.Exists(pngFile))
                {
                    errors.Add($"missing: {Path.GetRelativePath(pngRoot, pngFile)}");
                    continue;
                }

                try
                {
                    var bitmap = new BitmapImage(new Uri(pngFile));
                    var size = 16 * Scales[index];
                    if (bitmap.PixelWidth != size || bitmap.PixelHeight != size)
                        errors.Add($"wrong-size: {Path.GetRelativePath(pngRoot, pngFile)} is {bitmap.PixelWidth}x{bitmap.PixelHeight}, expected {size}x{size}");
                }
                catch (Exception exception)
                {
                    errors.Add($"unreadable: {Path.GetRelativePath(pngRoot, pngFile)} ({exception.Message})");
                }
            }
        }

        foreach (var pngFile in Directory.Exists(pngRoot) ? Directory.GetFiles(pngRoot, "*.png", SearchOption.AllDirectories) : [])
            if (!expected.Contains(Path.GetFullPath(pngFile)))
                errors.Add($"unexpected: {Path.GetRelativePath(pngRoot, pngFile)}");

        foreach (var error in errors)
            Console.WriteLine(error);
        Console.WriteLine($"SVG sources: {svgFiles.Count}");
        Console.WriteLine($"Expected PNGs: {svgFiles.Count * Buckets.Length}");
        Console.WriteLine($"Validation errors: {errors.Count}");
        return errors.Count == 0 ? 0 : 1;
    }

    private static int ValidateSvgBounds(string svgRoot)
    {
        const double epsilon = 0.01;
        var svgFiles = GetFiles(svgRoot, "*.svg");
        var violations = 0;
        var settings = new WpfDrawingSettings { IncludeRuntime = false, TextAsGeometry = true };
        foreach (var svgFile in svgFiles)
        {
            using var reader = new FileSvgReader(settings);
            var drawing = reader.Read(svgFile) ?? throw new InvalidDataException($"Could not render SVG: {svgFile}");
            var bounds = drawing.Bounds;
            if (bounds.IsEmpty || bounds.Left < -epsilon || bounds.Top < -epsilon || bounds.Right > 16 + epsilon || bounds.Bottom > 16 + epsilon)
            {
                Console.WriteLine($"outside: {GetSvgKey(svgFile, svgRoot)} bounds=({bounds.X:F3}, {bounds.Y:F3}, {bounds.Width:F3}, {bounds.Height:F3})");
                violations++;
            }
        }

        Console.WriteLine($"SVG sources: {svgFiles.Count}");
        Console.WriteLine($"Viewport bounds violations: {violations}");
        return violations == 0 ? 0 : 1;
    }

    private static int Rasterize(string root, string svgRoot, Dictionary<string, string> options)
    {
        var outputRoot = Path.GetFullPath(options.GetValueOrDefault("output", Path.Combine(root, "Png")));
        var bucketOption = options.GetValueOrDefault("bucket", "all");
        var indexes = bucketOption.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? Enumerable.Range(0, Buckets.Length).ToArray()
            : [Array.IndexOf(Buckets, bucketOption.StartsWith('@') ? bucketOption : $"@{bucketOption}x")];
        if (indexes.Any(index => index < 0))
            throw new ArgumentException("Bucket must be all, 1, 2, 3, 4, @1x, @2x, @3x or @4x.");

        var svgFiles = GetFiles(svgRoot, "*.svg");
        foreach (var index in indexes)
        {
            var size = 16 * Scales[index];
            foreach (var svgFile in svgFiles)
            {
                var relative = Path.ChangeExtension(Path.GetRelativePath(svgRoot, svgFile), ".png");
                var pngFile = Path.Combine(outputRoot, Buckets[index], relative);
                Directory.CreateDirectory(GetParentDirectory(pngFile));
                RenderSvg(svgFile, pngFile, size);
            }
            Console.WriteLine($"Rendered {svgFiles.Count} icons to {Path.Combine(outputRoot, Buckets[index])}.");
        }
        return 0;
    }

    private static int RasterizeLegacy(string root, string svgRoot, Dictionary<string, string> options)
    {
        if (!options.TryGetValue("legacy-root", out var configuredLegacyRoot)
            || !options.TryGetValue("destination", out var configuredDestination))
            throw new ArgumentException("rasterize-legacy requires explicit --legacy-root and --destination paths.");

        var legacyRoot = Path.GetFullPath(configuredLegacyRoot);
        var destinationRoot = Path.GetFullPath(configuredDestination);
        var mapFile = Path.GetFullPath(options.GetValueOrDefault("map", Path.Combine(root, "Tools", "IconPipeline", "legacy-icon-map.csv")));
        if (!Directory.Exists(legacyRoot) || !Directory.Exists(destinationRoot) || !File.Exists(mapFile))
            throw new DirectoryNotFoundException("Legacy root, destination root, and map file must exist.");

        var plan = new List<LegacyRasterEntry>();
        var destinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in ReadCsv(mapFile))
        {
            if (!row.TryGetValue("status", out var status) || !status.Equals("matched", StringComparison.OrdinalIgnoreCase))
                continue;
            if (!row.TryGetValue("legacy_png", out var legacyRelativePath)
                || !row.TryGetValue("svg_id", out var svgId)
                || !legacyRelativePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("A matched mapping row is missing a PNG path or SVG ID.");

            var sourcePath = ResolveContainedPath(legacyRoot, legacyRelativePath);
            var destinationPath = ResolveContainedPath(destinationRoot, legacyRelativePath);
            if (!File.Exists(sourcePath) || !File.Exists(destinationPath))
                throw new FileNotFoundException($"Mapped legacy source or destination is missing: {legacyRelativePath}");
            if (!destinations.Add(destinationPath))
                throw new InvalidDataException($"The map repeats a destination path: {legacyRelativePath}");

            var svgRelativePath = svgId.Replace('/', Path.DirectorySeparatorChar) + ".svg";
            var svgPath = ResolveContainedPath(svgRoot, svgRelativePath);
            if (!File.Exists(svgPath))
                throw new FileNotFoundException($"Mapped SVG is missing for {legacyRelativePath}: {svgId}");

            var (width, height) = ReadPngSize(sourcePath);
            if (!IsApprovedLegacySize(width, height))
                continue;

            plan.Add(new LegacyRasterEntry(legacyRelativePath, svgId, sourcePath, destinationPath, svgPath, width, height));
        }

        if (options.TryGetValue("svg-prefix", out var svgPrefix))
            plan = plan.Where(entry => entry.SvgId.StartsWith(svgPrefix, StringComparison.OrdinalIgnoreCase)).ToList();

        var planFile = Path.GetFullPath(options.GetValueOrDefault("plan-output", Path.Combine(Path.GetTempPath(), "TombLibLegacyRasterPlan.csv")));
        WriteCsv(planFile, ["legacy_png", "svg_id", "width", "height", "destination"], plan.Select(entry =>
            new[] { entry.RelativePath, entry.SvgId, entry.Width.ToString(CultureInfo.InvariantCulture), entry.Height.ToString(CultureInfo.InvariantCulture), entry.DestinationPath }));
        Console.WriteLine($"Matched map rows: {destinations.Count}");
        Console.WriteLine($"Eligible UI PNGs: {plan.Count}");
        Console.WriteLine($"Excluded by size, non-UI category, or filter: {destinations.Count - plan.Count}");
        foreach (var group in plan.GroupBy(entry => (entry.Width, entry.Height)).OrderBy(group => group.Key.Width).ThenBy(group => group.Key.Height))
            Console.WriteLine($"{group.Key.Width}x{group.Key.Height}: {group.Count()}");
        Console.WriteLine($"Plan: {planFile}");

        if (!options.TryGetValue("apply", out var applyValue) || !bool.TryParse(applyValue, out var apply) || !apply)
        {
            Console.WriteLine("Dry run only. Pass --apply true to replace the planned existing PNG files.");
            return 0;
        }

        foreach (var entry in plan)
        {
            var bitmap = RenderSvgCanvas(entry.SvgPath, entry.Width, entry.Height);
            File.WriteAllBytes(entry.DestinationPath, EncodePng(bitmap));
        }

        Console.WriteLine($"Replaced {plan.Count} existing legacy PNG files.");
        return 0;
    }

    private static int WriteContactSheet(string root, string svgRoot, Dictionary<string, string> options)
    {
        var legacyRoot = options.GetValueOrDefault("legacy-root");
        var fullLegacyRoot = legacyRoot is null ? null : Path.GetFullPath(legacyRoot);
        var legacyFiles = fullLegacyRoot is null ? [] : GetFiles(fullLegacyRoot, "*.png");
        var svgFiles = GetFiles(svgRoot, "*.svg");
        const int cardWidth = 380;
        const int cardHeight = 130;
        const int cardsPerRow = 4;
        const int cardsPerPage = 56;
        const int iconSize = 48;
        var entriesByCategory = new Dictionary<string, List<SheetEntry>>(StringComparer.OrdinalIgnoreCase);

        foreach (var svgFile in svgFiles)
        {
            var id = GetSvgKey(svgFile, svgRoot);
            var legacyFile = legacyFiles.FirstOrDefault(file => StringComparer.OrdinalIgnoreCase.Equals(GetLegacyKey(file, svgFiles, svgRoot), id));
            AddEntry(GetCategory(id), new SheetEntry(id, svgFile, legacyFile));
        }

        foreach (var legacyFile in legacyFiles)
        {
            var id = GetLegacyKey(legacyFile, svgFiles, svgRoot);
            var match = svgFiles.FirstOrDefault(file => StringComparer.OrdinalIgnoreCase.Equals(GetSvgKey(file, svgRoot), id));
            if (match is null)
                AddEntry(GetCategory(id), new SheetEntry(id, null, legacyFile));
        }

        var output = Path.GetFullPath(options.GetValueOrDefault("output", Path.Combine(root, "icon-contact-sheet.png")));
        var outputDirectory = GetParentDirectory(output);
        Directory.CreateDirectory(outputDirectory);
        var outputStem = Path.GetFileNameWithoutExtension(output);
        var pages = 0;
        foreach (var (category, entries) in entriesByCategory.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            for (var pageStart = 0; pageStart < entries.Count; pageStart += cardsPerPage)
            {
                var pageEntries = entries.Skip(pageStart).Take(cardsPerPage).ToArray();
                var pageNumber = pageStart / cardsPerPage + 1;
                var rows = (int)Math.Ceiling(pageEntries.Length / (double)cardsPerRow);
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    context.DrawRectangle(Brushes.White, null, new Rect(0, 0, cardWidth * cardsPerRow, cardHeight * rows));
                    for (var index = 0; index < pageEntries.Length; index++)
                    {
                        var entry = pageEntries[index];
                        var x = index % cardsPerRow * cardWidth;
                        var y = index / cardsPerRow * cardHeight;
                        DrawCard(context, x, y, cardWidth, cardHeight, entry.Id);
                        DrawIconSlot(context, entry.LegacyPath, x + 14, y + 46, iconSize, "origin/develop", "Legacy PNG", "no legacy match");
                        DrawIconSlot(context, entry.SvgPath, x + 190, y + 46, iconSize, "current", "SVG", "no SVG match");
                    }
                }

                var bitmap = new RenderTargetBitmap(cardWidth * cardsPerRow, cardHeight * rows, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                var pagePath = Path.Combine(outputDirectory, $"{outputStem}-{category}-{pageNumber:00}.png");
                SavePng(bitmap, pagePath);
                Console.WriteLine($"Wrote {pagePath} ({pageEntries.Length} icons).");
                pages++;
            }
        }

        Console.WriteLine($"Contact sheet pages: {pages}; SVGs: {svgFiles.Count}; legacy PNGs: {legacyFiles.Count}.");
        return 0;

        void AddEntry(string category, SheetEntry entry)
        {
            if (!entriesByCategory.TryGetValue(category, out var entries))
                entriesByCategory[category] = entries = [];
            entries.Add(entry);
        }
    }

    private static void DrawCard(DrawingContext context, int x, int y, int width, int height, string id)
    {
        var background = new SolidColorBrush(Color.FromRgb(47, 49, 54));
        background.Freeze();
        context.DrawRectangle(background, new Pen(Brushes.Gray, 1), new Rect(x, y, width, height));
        DrawText(context, id, 12, Brushes.White, x + 12, y + 9, width - 24, 30);
    }

    private static void DrawIconSlot(DrawingContext context, string? file, int x, int y, int size, string label, string heading, string missingText)
    {
        var dark = new SolidColorBrush(Color.FromRgb(35, 37, 41));
        dark.Freeze();
        context.DrawRectangle(dark, null, new Rect(x, y, size, size));
        DrawText(context, heading, 10, Brushes.Gainsboro, x, y - 16, size, 16);
        if (file is not null)
        {
            ImageSource image = Path.GetExtension(file).Equals(".svg", StringComparison.OrdinalIgnoreCase) ? LoadSvg(file) : LoadPng(file);
            var bounds = GetImageBounds(image, file);
            var scale = Math.Min(size / bounds.Width, size / bounds.Height);
            var drawWidth = bounds.Width * scale;
            var drawHeight = bounds.Height * scale;
            context.DrawImage(image, new Rect(x + (size - drawWidth) / 2.0, y + (size - drawHeight) / 2.0, drawWidth, drawHeight));
        }
        DrawText(context, file is null ? missingText : label, 9, file is null ? Brushes.Gray : Brushes.Gainsboro, x + size + 8, y + 3, 120, size - 4);
    }

    private static void DrawText(DrawingContext context, string text, double size, Brush brush, double x, double y, double maxWidth, double maxHeight)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), size, brush, 1.0) { MaxTextWidth = maxWidth, MaxTextHeight = maxHeight };
        context.DrawText(formatted, new Point(x, y));
    }

    private static DrawingImage LoadSvg(string svgFile)
    {
        var settings = new WpfDrawingSettings { IncludeRuntime = false, TextAsGeometry = true };
        using var reader = new FileSvgReader(settings);
        var drawing = reader.Read(svgFile) ?? throw new InvalidDataException($"Could not render SVG: {svgFile}");
        var viewport = new DrawingGroup();
        viewport.ClipGeometry = new RectangleGeometry(new Rect(0, 0, 16, 16));
        viewport.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, 16, 16))));
        viewport.Children.Add(drawing);
        var image = new DrawingImage(viewport);
        image.Freeze();
        return image;
    }

    private static BitmapImage LoadPng(string pngFile)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(pngFile);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private static Rect GetImageBounds(ImageSource image, string path) => image switch
    {
        DrawingImage when path.Replace('\\', '/').Contains("/Actions/Transform/", StringComparison.OrdinalIgnoreCase) => new Rect(0, 0, 75, 21),
        DrawingImage drawingImage => drawingImage.Drawing.Bounds,
        BitmapSource bitmap => new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
        _ => throw new InvalidDataException($"Unsupported contact sheet image type: {image.GetType().Name}")
    };

    private static void RenderSvg(string svgFile, string pngFile, int size)
    {
        SavePng(RenderSvgCanvas(svgFile, size, size), pngFile);
    }

    private static void SavePng(BitmapSource bitmap, string path)
    {
        using var stream = File.Create(path);
        var bytes = EncodePng(bitmap);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static RenderTargetBitmap RenderSvgCanvas(string svgFile, int width, int height)
    {
        var image = LoadSvg(svgFile);
        var bounds = image.Drawing.Bounds;
        var transformChart = width == 75 && height == 21;
        var scale = transformChart ? 1.0 : Math.Min(width / bounds.Width, height / bounds.Height);
        var drawWidth = transformChart ? width : bounds.Width * scale;
        var drawHeight = transformChart ? height : bounds.Height * scale;
        var x = (width - drawWidth) / 2.0;
        var y = (height - drawHeight) / 2.0;
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
            context.DrawImage(image, new Rect(x, y, drawWidth, drawHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        return bitmap;
    }

    private static byte[] EncodePng(BitmapSource bitmap)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static (int Width, int Height) ReadPngSize(string path)
    {
        var decoder = BitmapDecoder.Create(new Uri(path), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return (decoder.Frames[0].PixelWidth, decoder.Frames[0].PixelHeight);
    }

    private static bool IsApprovedLegacySize(int width, int height)
        => width == height && width is > 0 and <= 48 || width == 75 && height == 21;

    private static string ResolveContainedPath(string root, string relativePath)
    {
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalizedPath))
            throw new InvalidDataException($"Rooted input path is not allowed: {relativePath}");
        var fullPath = Path.GetFullPath(Path.Combine(root, normalizedPath));
        var rootPrefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Path escapes its configured root: {relativePath}");
        return fullPath;
    }

    private static List<Dictionary<string, string>> ReadCsv(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, true);
        var header = reader.ReadLine() ?? throw new InvalidDataException("CSV map is empty.");
        var columns = ParseCsvLine(header);
        var rows = new List<Dictionary<string, string>>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var values = ParseCsvLine(line);
            if (values.Count != columns.Count)
                throw new InvalidDataException("CSV map row has a different number of values than its header.");
            rows.Add(columns.Select((column, index) => (column, index)).ToDictionary(pair => pair.column, pair => values[pair.index], StringComparer.OrdinalIgnoreCase));
        }
        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"' && inQuotes && index + 1 < line.Length && line[index + 1] == '"')
            {
                field.Append('"');
                index++;
            }
            else if (character == '"')
                inQuotes = !inQuotes;
            else if (character == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
                field.Append(character);
        }
        if (inQuotes)
            throw new InvalidDataException("CSV map contains an unclosed quoted field.");
        fields.Add(field.ToString());
        return fields;
    }

    private static List<string> GetFiles(string root, string pattern)
    {
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Directory not found: {root}");
        return Directory.GetFiles(root, pattern, SearchOption.AllDirectories).OrderBy(file => file, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string GetSvgKey(string file, string svgRoot)
        => (Path.ChangeExtension(Path.GetRelativePath(svgRoot, file), null) ?? throw new InvalidDataException($"Invalid SVG path: {file}")).Replace('\\', '/');

    private static string GetLegacyKey(string file, List<string> svgFiles, string svgRoot)
    {
        var directory = Path.GetFileName(Path.GetDirectoryName(file)) ?? "";
        var category = directory.StartsWith("icons_", StringComparison.OrdinalIgnoreCase) ? directory[6..] : "";
        var stem = Path.GetFileNameWithoutExtension(file).Replace(" copy", "", StringComparison.OrdinalIgnoreCase);
        stem = Regex.Replace(stem, "[-_](16|24|32|48)$", "", RegexOptions.IgnoreCase).Trim().Replace(' ', '_');
        if (category.Equals("transform", StringComparison.OrdinalIgnoreCase))
        {
            if (stem.StartsWith("transform_", StringComparison.OrdinalIgnoreCase))
                stem = stem[10..];
            return $"Actions/Transform/{ToPascalCase(stem)}";
        }

        if (category.Equals("transport", StringComparison.OrdinalIgnoreCase) || stem.StartsWith("transport-", StringComparison.OrdinalIgnoreCase))
        {
            var transportName = stem.StartsWith("transport-", StringComparison.OrdinalIgnoreCase) ? stem[10..] : stem;
            return $"Actions/Transport/{ToPascalCase(transportName)}";
        }

        if (string.IsNullOrEmpty(category))
        {
            var prefixCategory = Categories.FirstOrDefault(candidate => stem.StartsWith(candidate + "_", StringComparison.OrdinalIgnoreCase));
            if (prefixCategory is not null)
            {
                category = prefixCategory;
                stem = stem[(prefixCategory.Length + 1)..];
            }
            else
                category = "Misc";
        }
        else
        {
            var prefix = category + "_";
            if (stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                stem = stem[prefix.Length..];
        }
        var key = $"{ToCategoryName(category)}/{stem}";
        if (key.Equals("General/search-and-replace", StringComparison.OrdinalIgnoreCase))
            key = "General/Find-and-replace";
        else if (key.Equals("General/cut", StringComparison.OrdinalIgnoreCase))
            key = "Actions/cut";
        else if (key.Equals("Transport/plus", StringComparison.OrdinalIgnoreCase) || key.Equals("Transport/transport-plus", StringComparison.OrdinalIgnoreCase))
            key = "Actions/Transport/Plus";
        else if (file.Replace('\\', '/').EndsWith("/WadTool/Resources/play-16.png", StringComparison.OrdinalIgnoreCase))
            key = "Actions/Transport/Play";
        var exactMatch = svgFiles.FirstOrDefault(svg => StringComparer.OrdinalIgnoreCase.Equals(GetSvgKey(svg, svgRoot), key));
        if (exactMatch is not null)
            return GetSvgKey(exactMatch, svgRoot);

        var basenameMatches = svgFiles.Where(svg => StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileNameWithoutExtension(svg), stem)).ToList();
        return basenameMatches.Count == 1 ? GetSvgKey(basenameMatches[0], svgRoot) : key;
    }

    private static string GetCategory(string iconId)
        => iconId.Split('/', 2)[0];

    private static string ToPascalCase(string value)
        => string.Concat(Regex.Split(value, "[-_]+")
            .Where(part => part.Length > 0)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private static string ToCategoryName(string category) => category.ToLowerInvariant() switch
    {
        "actions" => "Actions",
        "general" => "General",
        "misc" => "Misc",
        "objects" => "Objects",
        "sectortype" => "Sectortype",
        "texture" => "Texture",
        "toolbox" => "Toolbox",
        "transport" => "Transport",
        _ => category
    };

    private static void PrintCategoryMetrics(List<string> svgFiles, string svgRoot)
    {
        foreach (var category in Categories)
        {
            var count = svgFiles.Count(file => StringComparer.OrdinalIgnoreCase.Equals(GetSvgKey(file, svgRoot).Split('/')[0], category));
            Console.WriteLine($"{category}: {count}");
        }
    }

    private static void WriteCsv(string path, string[] headers, IEnumerable<string[]> rows)
    {
        Directory.CreateDirectory(GetParentDirectory(path));
        using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
        writer.WriteLine(string.Join(',', headers.Select(EscapeCsv)));
        foreach (var row in rows)
            writer.WriteLine(string.Join(',', row.Select(EscapeCsv)));
    }

    private static string EscapeCsv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

    private static string GetParentDirectory(string path)
        => Path.GetDirectoryName(path) ?? throw new InvalidDataException($"Path has no parent directory: {path}");

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length; index++)
        {
            if (!args[index].StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
                throw new ArgumentException($"Expected an option and value near '{args[index]}'.");
            options[args[index][2..]] = args[++index];
        }
        return options;
    }

    private static string FindProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "TombLib.Icons.csproj")))
                return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Could not find TombLib.Icons.csproj. Pass --root.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("IconPipeline commands:");
        Console.WriteLine("  inventory [--legacy-root path] [--output file.csv] [--root TombLib.Icons]");
        Console.WriteLine("  validate-svg-bounds [--root TombLib.Icons]");
        Console.WriteLine("  validate-png [--png-root path] [--root TombLib.Icons]");
        Console.WriteLine("  rasterize [--bucket all|1|2|3|4] [--output Png] [--root TombLib.Icons]");
        Console.WriteLine("  rasterize-legacy --legacy-root path --destination repository-root [--map file.csv] [--apply true]");
        Console.WriteLine("  contact-sheet [--legacy-root path] [--output file.png] [--root TombLib.Icons]");
    }

    private sealed record LegacyRasterEntry(string RelativePath, string SvgId, string SourcePath, string DestinationPath, string SvgPath, int Width, int Height);
    private sealed record SheetEntry(string Id, string? SvgPath, string? LegacyPath);
}
