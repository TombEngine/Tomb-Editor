using TombLib.LevelData;

namespace TombLib.Test
{
    [TestClass]
    public class WayPointInstanceTests
    {
        [TestMethod]
        public void WayPoint_AutoNaming_DefaultName()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();

            // Assert
            Assert.AreEqual("WayPoint_0", wayPoint.Name, "Default WayPoint name should be 'WayPoint_0'");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_CustomBaseName()
        {
            // Arrange
            var wayPoint = new WayPointInstance();

            // Act
            wayPoint.Name = "Camera";
            wayPoint.Number = 0;

            // Assert
            Assert.AreEqual("Camera_0", wayPoint.Name, "Custom name should be 'Camera_0'");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_SequenceChange()
        {
            // Arrange
            var wayPoint = new WayPointInstance();
            wayPoint.Name = "MyPath";

            // Act
            wayPoint.Number = 5;

            // Assert
            Assert.AreEqual("MyPath_5", wayPoint.Name, "Name should update to 'MyPath_5' when number changes");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_NumberChange()
        {
            // Arrange
            var wayPoint = new WayPointInstance();
            wayPoint.Name = "Camera";
            wayPoint.Number = 3;

            // Act
            wayPoint.Number = 7;

            // Assert
            Assert.AreEqual("Camera_7", wayPoint.Name, "Name should update to 'Camera_7' when number changes to 7");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_PreservesBaseName()
        {
            // Arrange
            var wayPoint = new WayPointInstance();
            wayPoint.Name = "PatrolPath";
            wayPoint.Number = 0;

            // Act
            wayPoint.Number = 10;

            // Assert
            Assert.AreEqual("PatrolPath_10", wayPoint.Name, "Base name 'PatrolPath' should be preserved");
        }

        [TestMethod]
        public void WayPoint_DefaultPathType()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();

            // Assert
            Assert.AreEqual(PathType.Linear, wayPoint.PathType, "Default PathType should be Linear");
        }

        [TestMethod]
        public void WayPoint_PathTypeCanBeSet()
        {
            // Arrange
            var wayPoint = new WayPointInstance();

            // Act
            wayPoint.PathType = PathType.Bezier;

            // Assert
            Assert.AreEqual(PathType.Bezier, wayPoint.PathType, "PathType should be settable to Bezier");
        }

        [TestMethod]
        public void WayPoint_RotationXClamping()
        {
            // Arrange
            var wayPoint = new WayPointInstance();

            // Act & Assert
            wayPoint.RotationX = 100;
            Assert.AreEqual(90, wayPoint.RotationX, "RotationX should be clamped to 90");

            wayPoint.RotationX = -100;
            Assert.AreEqual(-90, wayPoint.RotationX, "RotationX should be clamped to -90");
        }

        [TestMethod]
        public void WayPoint_RotationYWrapping()
        {
            // Arrange
            var wayPoint = new WayPointInstance();

            // Act
            wayPoint.RotationY = 400;

            // Assert
            Assert.AreEqual(40, wayPoint.RotationY, 0.01, "RotationY should wrap around 360 degrees");
        }

        [TestMethod]
        public void WayPoint_RollWrapping()
        {
            // Arrange
            var wayPoint = new WayPointInstance();

            // Act
            wayPoint.Roll = 720;

            // Assert
            Assert.AreEqual(0, wayPoint.Roll, 0.01, "Roll should wrap around 360 degrees");
        }
    }
}
