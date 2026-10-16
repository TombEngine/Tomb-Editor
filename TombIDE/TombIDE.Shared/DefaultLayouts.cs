using System.Collections.Generic;
using System.Drawing;
using TombIDE.Shared.Docking;

namespace TombIDE.Shared
{
	public static class DefaultLayouts
	{
		public static readonly DockPanelState DummyLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Left,
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "ContentExplorer" },
							VisibleContent = "ContentExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Bottom,
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "SearchResults" },
							VisibleContent = "SearchResults"
						}
					}
				}
			}
		};

		public static readonly DockPanelState ClassicScriptLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Size = new Size(0, 0),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Left,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "ContentExplorer" },
							VisibleContent = "ContentExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Bottom,
					Size = new Size(300, 200),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string>
							{
								"ReferenceBrowser",
								"CompilerLogs",
								"SearchResults"
							},

							VisibleContent = "ReferenceBrowser"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Bottom,
					Size = new Size(320, 220),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string>
							{
								"SearchResults",
								"LuaDiagnostics",
								"LuaReferencesResults"
							},

							VisibleContent = "LuaDiagnostics"
						}
					}
				}
			}
		};

		public static readonly DockPanelState LuaLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Size = new Size(0, 0),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						}
					}
				}
			}
		};

		public static readonly DockPanelState GameFlowScriptLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Size = new Size(0, 0),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Left,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "ContentExplorer" },
							VisibleContent = "ContentExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						}
					}
				}
			}
		};

		public static readonly DockPanelState TRXLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Size = new Size(0, 0),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Left,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "ContentExplorer" },
							VisibleContent = "ContentExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Size = new Size(250, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						}
					}
				}
			}
		};
	}
}
