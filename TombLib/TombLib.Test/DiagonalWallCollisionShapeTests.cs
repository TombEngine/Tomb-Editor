using System.Globalization;
using System.Reflection;
using TombLib.LevelData;
using TombLib.LevelData.Compilers;
using TombLib.LevelData.Compilers.TombEngine;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;

namespace TombLib.Test;

[TestClass]
public class DiagonalWallCollisionShapeTests
{
    private static readonly Type TombEngineRoomSectorShapeType = typeof(LevelCompilerTombEngine)
        .GetNestedType("RoomSectorShape", BindingFlags.NonPublic)!;
    private static readonly Type ClassicRoomSectorShapeType = typeof(LevelCompilerClassicTR)
        .GetNestedType("RoomSectorShape", BindingFlags.NonPublic)!;

    [DataTestMethod]
    [DataRow(DiagonalSplit.XnZn)]
    [DataRow(DiagonalSplit.XnZp)]
    [DataRow(DiagonalSplit.XpZn)]
    [DataRow(DiagonalSplit.XpZp)]
    public void TombEngineRoomSectorShape_FlattensDiagonalWallFloorCollision(DiagonalSplit diagonalSplit)
    {
        AssertDiagonalWallCollisionIsFlattened(TombEngineRoomSectorShapeType, diagonalSplit, isFloor: true);
    }

    [DataTestMethod]
    [DataRow(DiagonalSplit.XnZn)]
    [DataRow(DiagonalSplit.XnZp)]
    [DataRow(DiagonalSplit.XpZn)]
    [DataRow(DiagonalSplit.XpZp)]
    public void TombEngineRoomSectorShape_FlattensDiagonalWallCeilingCollision(DiagonalSplit diagonalSplit)
    {
        AssertDiagonalWallCollisionIsFlattened(TombEngineRoomSectorShapeType, diagonalSplit, isFloor: false);
    }

    [TestMethod]
    public void TombEngineRoomSectorShape_LeavesNonWallDiagonalSplitUntouched()
    {
        AssertNonWallDiagonalCollisionIsUntouched(TombEngineRoomSectorShapeType);
    }

    [DataTestMethod]
    [DataRow(DiagonalSplit.XnZn)]
    [DataRow(DiagonalSplit.XnZp)]
    [DataRow(DiagonalSplit.XpZn)]
    [DataRow(DiagonalSplit.XpZp)]
    public void ClassicRoomSectorShape_FlattensDiagonalWallFloorCollision(DiagonalSplit diagonalSplit)
    {
        AssertDiagonalWallCollisionIsFlattened(ClassicRoomSectorShapeType, diagonalSplit, isFloor: true);
    }

    [DataTestMethod]
    [DataRow(DiagonalSplit.XnZn)]
    [DataRow(DiagonalSplit.XnZp)]
    [DataRow(DiagonalSplit.XpZn)]
    [DataRow(DiagonalSplit.XpZp)]
    public void ClassicRoomSectorShape_FlattensDiagonalWallCeilingCollision(DiagonalSplit diagonalSplit)
    {
        AssertDiagonalWallCollisionIsFlattened(ClassicRoomSectorShapeType, diagonalSplit, isFloor: false);
    }

    [TestMethod]
    public void ClassicRoomSectorShape_LeavesNonWallDiagonalSplitUntouched()
    {
        AssertNonWallDiagonalCollisionIsUntouched(ClassicRoomSectorShapeType);
    }

    private static Sector CreateDiagonalWallSector(DiagonalSplit diagonalSplit, bool isFloor)
    {
        var sector = new Sector(0, 0)
        {
            Type = SectorType.Wall
        };

        var surface = new SectorSurface
        {
            DiagonalSplit = diagonalSplit,
            XnZn = 28,
            XnZp = 4,
            XpZn = 16,
            XpZp = 40
        };

        if (isFloor)
            sector.Floor = surface;
        else
            sector.Ceiling = surface;

        return sector;
    }

    private static (string FlatHeightField, string FirstFlattenedField, string SecondFlattenedField) GetFlatTriangleFields(DiagonalSplit diagonalSplit)
        => diagonalSplit switch
        {
            DiagonalSplit.XnZn => ("HeightXpZp", "HeightXnZp", "HeightXpZn"),
            DiagonalSplit.XnZp => ("HeightXpZn", "HeightXnZn", "HeightXpZp"),
            DiagonalSplit.XpZn => ("HeightXnZp", "HeightXnZn", "HeightXpZp"),
            DiagonalSplit.XpZp => ("HeightXnZn", "HeightXnZp", "HeightXpZn"),
            _ => throw new ArgumentOutOfRangeException(nameof(diagonalSplit))
        };

    private static void AssertDiagonalWallCollisionIsFlattened(Type roomSectorShapeType, DiagonalSplit diagonalSplit, bool isFloor)
    {
        var sector = CreateDiagonalWallSector(diagonalSplit, isFloor);
        var shape = CreateRoomSectorShape(roomSectorShapeType, sector, isFloor);
        var (flatHeightField, firstFlattenedField, secondFlattenedField) = GetFlatTriangleFields(diagonalSplit);
        int flatHeight = GetField<int>(roomSectorShapeType, shape, flatHeightField);

        Assert.AreEqual(flatHeight, GetField<int>(roomSectorShapeType, shape, firstFlattenedField));
        Assert.AreEqual(flatHeight, GetField<int>(roomSectorShapeType, shape, secondFlattenedField));
        Assert.AreEqual(0, GetField<int>(roomSectorShapeType, shape, "DiagonalStep"));
    }

    private static void AssertNonWallDiagonalCollisionIsUntouched(Type roomSectorShapeType)
    {
        var sector = new Sector(0, 0)
        {
            Type = SectorType.Floor,
            Floor = new SectorSurface
            {
                DiagonalSplit = DiagonalSplit.XpZn,
                XnZn = 28,
                XnZp = 4,
                XpZn = 16,
                XpZp = 40
            }
        };

        var shape = CreateRoomSectorShape(roomSectorShapeType, sector, floor: true);
        Assert.AreNotEqual(0, GetField<int>(roomSectorShapeType, shape, "DiagonalStep"));
    }

    private static object CreateRoomSectorShape(Type roomSectorShapeType, Sector sector, bool floor)
        => Activator.CreateInstance(roomSectorShapeType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: new object[] { sector, floor, Room.RoomConnectionType.NoPortal, sector.IsAnyWall },
            culture: CultureInfo.InvariantCulture)!;

    private static T GetField<T>(Type roomSectorShapeType, object instance, string fieldName)
        => (T)roomSectorShapeType
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(instance)!;
}
