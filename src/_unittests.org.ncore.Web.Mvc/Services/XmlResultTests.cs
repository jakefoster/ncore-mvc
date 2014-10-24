using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using org.ncore.Diagnostics;
using org.ncore.Extensions;
using org.ncore.Web.Mvc.Services;

namespace _unittests.org.ncore.Web.Mvc.Services
{
    /// <summary>
    /// Summary description for XmlResult
    /// </summary>
    [TestClass]
    public class XmlResultTests
    {
        public XmlResultTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        // TODO: Need more tests.  Also, something expository to demonstrate how this is actually used 
        //  in a controller (since it's not really obvious from the real tests.)  JF
        [TestMethod]
        public void ExecuteResult()
        {
            // ARRANGE
            XNamespace ns = "http://example.com/schema";
            XDocument document = new XDocument(
                            new XElement( ns + "Response",
                                new XElement( ns + "Info", new XText( "SomeStuff" ) ) ) );

            var mockControllerContext = new Mock<ControllerContext>( MockBehavior.Strict );
            mockControllerContext.SetupProperty( c => c.HttpContext.Response.ContentType );
            mockControllerContext.Setup( c => c.HttpContext.Response.ClearContent() );
            mockControllerContext.Setup( c => c.HttpContext.Response.Write( It.IsAny<string>() ) );
            mockControllerContext.Setup( c => c.HttpContext.Response.End() );

            // ACT
            XmlResult result = new XmlResult( document )
                {
                    Encoding = Encoding.UTF8,
                    StatusCode = 200,
                    StatusDescription = "OK"
                };

            result.ExecuteResult( mockControllerContext.Object );

            // ASSERT
            Assert.AreEqual( "text/xml", result.ContentType );
            Assert.AreEqual( Encoding.UTF8.EncodingName, result.Encoding.EncodingName );
            Assert.AreEqual( 200, result.StatusCode );
            Assert.AreEqual( "OK", result.StatusDescription );
            Assert.AreEqual( document, result.ResponseXml );
        }
    }
}
