#nullable enable

using System;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>Node editor layout settings taken from the editor configuration.</summary>
    /// <param name="GridSize">Number of grid cells along each canvas axis.</param>
    /// <param name="GridStep">Size of one grid cell in pixels.</param>
    /// <param name="DefaultNodeWidth">Width of newly created nodes.</param>
    /// <param name="LinksAsRopes">Draw links as curves instead of straight lines.</param>
    public sealed record NodeEditorOptions(int GridSize, double GridStep, int DefaultNodeWidth, bool LinksAsRopes)
    {
        public double CanvasExtent => GridSize * GridStep;

        public static NodeEditorOptions FromConfiguration(Configuration configuration) => new(
            Math.Max(configuration.NodeEditor_Size, 16),
            Math.Max(configuration.NodeEditor_GridStep, 1),
            configuration.NodeEditor_DefaultNodeWidth,
            configuration.NodeEditor_LinksAsRopes);
    }
}
