using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Log4Flock.Web;
using Log4Flock.Web.Controllers;

namespace Log4Flock.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            StackTraceController controller = new StackTraceController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}
