using System.Linq;
using System.Numerics;
using System.Reflection;
using TombLib.LevelData;

namespace TombLib.Test;

[TestClass]
public class ObjectGroupTests
{
	[TestMethod]
	public void Clone_PreservesRootObjectAndGroupRotation()
	{
		var level = Level.CreateSimpleLevel();
		var room = level.Rooms[0];

		var firstLight = new LightInstance(LightType.Point)
		{
			Position = new Vector3(1024.0f, 0.0f, 1024.0f),
			Color = new Vector3(1.0f, 0.0f, 0.0f)
		};

		var secondLight = new LightInstance(LightType.Point)
		{
			Position = new Vector3(2048.0f, 0.0f, 1024.0f),
			Color = new Vector3(0.0f, 1.0f, 0.0f)
		};

		room.AddObject(level, firstLight);
		room.AddObject(level, secondLight);

		var group = new ObjectGroup(firstLight)
		{
			secondLight
		};

		group.RotationY = 45.0f;

		var firstEnumeratedObject = group.First();
		var expectedRootObject = group.First(obj => obj != firstEnumeratedObject);
		SetRootObject(group, expectedRootObject);

		var clonedGroup = (ObjectGroup)group.Clone();
		var clonedExpectedRoot = clonedGroup.First(obj => ((IColorable)obj).Color == ((IColorable)expectedRootObject).Color);

		Assert.AreSame(clonedExpectedRoot, clonedGroup.RootObject);
		Assert.AreEqual(((IColorable)expectedRootObject).Color, clonedGroup.Color);
		Assert.AreEqual(group.RotationY, clonedGroup.RotationY);
	}

	private static void SetRootObject(ObjectGroup group, PositionBasedObjectInstance rootObject)
	{
		typeof(ObjectGroup)
			.GetField("_rootObject", BindingFlags.Instance | BindingFlags.NonPublic)
			!.SetValue(group, rootObject);
	}
}
