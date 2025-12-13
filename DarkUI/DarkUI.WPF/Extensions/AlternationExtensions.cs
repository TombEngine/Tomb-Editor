using System.Reflection;
using System.Windows.Controls;

namespace DarkUI.WPF.Extensions;

public static class AlternationExtensions
{
	private static readonly MethodInfo? SetAlternationIndexMethod;

	static AlternationExtensions()
		=> SetAlternationIndexMethod = typeof(ItemsControl).GetMethod("SetAlternationIndex", BindingFlags.Static | BindingFlags.NonPublic);

	public static int SetAlternationIndexRecursively(this ItemsControl control, int firstAlternationIndex)
	{
		int alternationCount = control.AlternationCount;

		if (alternationCount == 0)
			return 0;

		foreach (object? item in control.Items)
		{
			if (control.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem container)
			{
				int nextAlternation = firstAlternationIndex++ % alternationCount;
				SetAlternationIndexMethod?.Invoke(null, new object[] { container, nextAlternation });

				if (container.IsExpanded)
					firstAlternationIndex = SetAlternationIndexRecursively(container, firstAlternationIndex);
			}
		}

		return firstAlternationIndex;
	}
}
