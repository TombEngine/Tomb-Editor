using TombLib.LevelData;

namespace TombLib.Test
{
    [TestClass]
    public class WayPointInstanceTests
    {
        [TestMethod]
        public void WayPoint_DefaultType()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();

            // Assert
            Assert.AreEqual(WayPointType.Point, wayPoint.Type, "Default Type should be Point");
        }

        [TestMethod]
        public void WayPoint_TypeCanBeSet()
        {
            // Arrange
            var wayPoint = new WayPointInstance();

            // Act
            wayPoint.Type = WayPointType.Bezier;

            // Assert
            Assert.AreEqual(WayPointType.Bezier, wayPoint.Type, "Type should be settable to Bezier");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_SingularType()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Circle;
            wayPoint.Name = "Patrol";

            // Assert
            Assert.AreEqual("Patrol", wayPoint.Name, "Singular type should use base name only");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_MultiPointType()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Linear;
            wayPoint.Name = "Path";
            wayPoint.Number = 5;

            // Assert
            Assert.AreEqual("Path_5", wayPoint.Name, "Multi-point type should use Name_Number format");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_NumberChangeLinear()
        {
            // Arrange
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Linear;
            wayPoint.Name = "Camera";
            wayPoint.Number = 3;

            // Act
            wayPoint.Number = 7;

            // Assert
            Assert.AreEqual("Camera_7", wayPoint.Name, "Name should update to 'Camera_7' when number changes to 7");
        }

        [TestMethod]
        public void WayPoint_AutoNaming_TypeChangeToSingular()
        {
            // Arrange
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Bezier;
            wayPoint.Name = "Target";
            wayPoint.Number = 3;
            Assert.AreEqual("Target_3", wayPoint.Name, "Initially should be Target_3");

            // Act
            wayPoint.Type = WayPointType.Ellipse;

            // Assert
            Assert.AreEqual("Target", wayPoint.Name, "After changing to singular type, name should be just 'Target'");
        }

        [TestMethod]
        public void WayPoint_RequiresRadius_Circle()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Circle;

            // Assert
            Assert.IsTrue(wayPoint.RequiresRadius(), "Circle should require radius");
        }

        [TestMethod]
        public void WayPoint_RequiresTwoRadii_Ellipse()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Ellipse;

            // Assert
            Assert.IsTrue(wayPoint.RequiresTwoRadii(), "Ellipse should require two radii");
        }

        [TestMethod]
        public void WayPoint_IsSingularType_Point()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Point;

            // Assert
            Assert.IsTrue(wayPoint.IsSingularType(), "Point should be a singular type");
        }

        [TestMethod]
        public void WayPoint_IsSingularType_Linear()
        {
            // Arrange & Act
            var wayPoint = new WayPointInstance();
            wayPoint.Type = WayPointType.Linear;

            // Assert
            Assert.IsFalse(wayPoint.IsSingularType(), "Linear should not be a singular type");
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
