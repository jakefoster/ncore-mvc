using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.ncore.Exceptions;
using org.ncore.Web.Mvc.Services;

namespace _unittests.org.ncore.Web.Mvc.Services
{
    /// <summary>
    /// Summary description for XmlConfirmationTests
    /// </summary>
    [TestClass]
    public class XmlConfirmationTests
    {
        public XmlConfirmationTests()
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

        // TODO: Need more tests.  JF
        [TestMethod]
        public void ExecuteResult()
        {
            // ARRANGE
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add( "item1", "value1" );

            XmlConfirmation confirmation = new XmlConfirmation()
                                               {
                                                   Action = "ExecuteResult",
                                                   Key = "123",
                                                   Message = "Success",
                                                   Metadata = metadata
                                               };

            // ACT
           XDocument responseXml = confirmation.BuildXml();

            // ASSERT
            Assert.IsNotNull( responseXml );
            XNamespace ns = "http://schemas.ncore.org/Web/Mvc/Service/Confirmation/1/";
            Assert.AreEqual( ns + "Confirmation", responseXml.Root.Name );
            Assert.AreEqual( "123", responseXml.Root.Attribute( "Key" ).Value );
            Assert.AreEqual( "ExecuteResult", responseXml.Root.Attribute( "Action" ).Value );
            Assert.AreEqual( "1", responseXml.Root.Attribute( "Revision" ).Value );

            Assert.AreEqual( 2, responseXml.Root.Elements().Count() );
            Assert.AreEqual( ns + "Message", responseXml.Root.Elements().ElementAt( 0 ).Name );
            Assert.AreEqual( ns + "Metadata", responseXml.Root.Elements().ElementAt( 1 ).Name );

            XElement messageNode = responseXml.Root.Element( ns + "Message" );
            Assert.AreEqual( "Success", messageNode.Value );

            XElement metadataNode = responseXml.Root.Element( ns + "Metadata" );
            Assert.AreEqual( 1, metadataNode.Elements().Count() );
            Assert.AreEqual( ns + "Item", metadataNode.Elements().ElementAt( 0 ).Name );

            Assert.AreEqual( "item1", metadataNode.Element( ns + "Item" ).Attribute( "Type" ).Value );
            Assert.AreEqual( "value1", metadataNode.Element( ns + "Item" ).Value );
        }
    }
}
