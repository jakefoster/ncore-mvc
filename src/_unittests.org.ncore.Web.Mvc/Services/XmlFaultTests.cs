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
using org.ncore.Exceptions;
using org.ncore.Diagnostics;
using org.ncore.Extensions;
using org.ncore.Web.Mvc.Services;

namespace _unittests.org.ncore.Web.Mvc.Services
{
    /// <summary>
    /// Summary description for XmlFaultTests
    /// </summary>
    [TestClass]
    public class XmlFaultTests
    {
        public XmlFaultTests()
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
            XmlFault fault = null;
            try
            {
                throw new RuntimeException( "Something *really* bad happened.", "Something bad happened." );
            }
            catch( Exception ex )
            {
                Dictionary<string,string> metadata = new Dictionary<string, string>();
                metadata.Add( "item1", "value1" );

                fault = new XmlFault()
                {
                    Exception = ex,
                    Action = "ExecuteResult",
                    Key = "123",
                    Metadata = metadata
                };
            }

            // ACT
            XDocument responseXml = fault.BuildXml( true );

            // ASSERT
            Assert.IsNotNull( responseXml );
            XNamespace ns = "http://schemas.ncore.org/Web/Mvc/Service/Fault/1/";
            Assert.AreEqual( ns + "Fault", responseXml.Root.Name );
            Assert.AreEqual( "123", responseXml.Root.Attribute( "Key" ).Value );
            Assert.AreEqual( "ExecuteResult", responseXml.Root.Attribute( "Action" ).Value );
            Assert.AreEqual( "1", responseXml.Root.Attribute( "Revision" ).Value );

            Assert.AreEqual( 2, responseXml.Root.Elements().Count() );
            Assert.AreEqual( ns + "Exception", responseXml.Root.Elements().ElementAt( 0 ).Name );
            Assert.AreEqual( ns + "Metadata", responseXml.Root.Elements().ElementAt( 1 ).Name );

            XElement exceptionNode = responseXml.Root.Element( ns + "Exception" );
            Assert.AreEqual( 7, exceptionNode.Elements().Count() );
            Assert.AreEqual( ns + "Type", exceptionNode.Elements().ElementAt( 0 ).Name );
            Assert.AreEqual( ns + "Message", exceptionNode.Elements().ElementAt( 1 ).Name );
            Assert.AreEqual( ns + "Instructions", exceptionNode.Elements().ElementAt( 2 ).Name );
            Assert.AreEqual( ns + "Source", exceptionNode.Elements().ElementAt( 3 ).Name );
            Assert.AreEqual( ns + "Operation", exceptionNode.Elements().ElementAt( 4 ).Name );
            Assert.AreEqual( ns + "StackTrace", exceptionNode.Elements().ElementAt( 5 ).Name );
            Assert.AreEqual( ns + "InnerException", exceptionNode.Elements().ElementAt( 6 ).Name );

            Assert.AreEqual( "org.ncore.Exceptions.RuntimeException", exceptionNode.Element( ns + "Type" ).Value );
            Assert.AreEqual( "Something *really* bad happened.", exceptionNode.Element( ns + "Message" ).Value );
            Assert.AreEqual( "Something bad happened.", exceptionNode.Element( ns + "Instructions" ).Value );
            Assert.AreEqual( "_unittests.org.ncore.Web.Mvc", exceptionNode.Element( ns + "Source" ).Value );
            Assert.AreEqual( "Void ExecuteResult()", exceptionNode.Element( ns + "Operation" ).Value );
            // TODO: Would be best done with RegEx.  JKF
            Assert.IsTrue(  exceptionNode.Element( ns + "StackTrace" ).Value.Contains( @"at _unittests.org.ncore.Web.Mvc.Services.XmlFaultTests.ExecuteResult() in " ) );
            Assert.IsTrue( exceptionNode.Element( ns + "StackTrace" ).Value.Contains( @"\_unittests.org.ncore.Web.Mvc\Services\XmlFaultTests.cs:line " ) );
            Assert.IsTrue( exceptionNode.Element( ns + "InnerException" ).IsEmpty );

            XElement metadataNode = responseXml.Root.Element( ns + "Metadata" );
            Assert.AreEqual( 1, metadataNode.Elements().Count() );
            Assert.AreEqual( ns + "Item", metadataNode.Elements().ElementAt( 0 ).Name );

            Assert.AreEqual( "item1", metadataNode.Element( ns + "Item" ).Attribute( "Type" ).Value );
            Assert.AreEqual( "value1", metadataNode.Element( ns + "Item" ).Value );

        }
    }
}
