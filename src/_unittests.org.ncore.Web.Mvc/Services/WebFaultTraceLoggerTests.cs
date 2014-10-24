using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _unittests.org.ncore.Web.Mvc.Utility;
using org.ncore.Exceptions;
using Moq;
using org.ncore.Diagnostics;
using org.ncore.Extensions;
using org.ncore.Web.Mvc.Services;

namespace _unittests.org.ncore.Web.Mvc.Services
{
    /// <summary>
    /// Summary description for WebFaultTraceLoggerTests
    /// </summary>
    [TestClass]
    public class WebFaultTraceLoggerTests
    {
        public WebFaultTraceLoggerTests()
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
            get { return testContextInstance; }
            set { testContextInstance = value; }
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

        [TestMethod]
        public void LogFault()
        {
            StringBuilder builder = new StringBuilder();
            UnitTestTraceListener traceListener = new UnitTestTraceListener( builder );
            Trace.Listeners.Add( traceListener );
            try
            {
                // ARRANGE
                XmlFault fault = null;
                Exception exception = null;
                try
                {
                    throw new RuntimeException( "Something *really* bad happened.", "Something bad happened." );
                }
                catch( Exception ex )
                {
                    exception = ex;

                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    metadata.Add( "item1", "value1" );

                    fault = new XmlFault()
                                {
                                    Exception = ex,
                                    Action = "ExecuteResult",
                                    Key = "123",
                                    Metadata = metadata
                                };
                }

                ExceptionContextBuilder exceptionContextBuilder = _getExceptionContextBuilder( exception );

                WebFaultTraceLogger logger = new WebFaultTraceLogger();
                logger.Initialize( exceptionContextBuilder.ExceptionContext, fault );

                // ACT
                logger.LogFault();

                // ASSERT
                // NOTE: I don't need to test the Spy output (it's already being tested in the Spy unit tests),
                //  really I just need to test the WebFaultTraceLogger XML.  JF
                StringReader reader = new StringReader( builder.ToString() );
                reader.ReadLine();
                reader.ReadLine();
                reader.ReadLine();
                XDocument document = XDocument.Parse( reader.ReadToEnd().Replace( "<<", string.Empty ) );

                Assert.AreEqual( "Fault", document.Root.Elements().First().Name.LocalName );
                // NOTE: No real need to examine the content of the Fault XML since it's already being
                //  unit tested in the XmlFault tests.  JF

                Assert.AreEqual( Environment.MachineName, document.Root.Element( "ServerName" ).Value );
                Assert.AreEqual( "MyController.MyAction", document.Root.Element( "ServiceName" ).Value );
                Assert.AreEqual( "{controller}/{action}/{id}", document.Root.Element( "RouteUrl" ).Value );

                Assert.AreEqual( "/foo/bar", document.Root.Element( "Path" ).Value );
                Assert.AreEqual( "/foo/bar", document.Root.Element( "FilePath" ).Value );
                Assert.AreEqual( "/", document.Root.Element( "ApplicationPath" ).Value );
                Assert.AreEqual( "/foo/bar", document.Root.Element( "CurrentExecutionFilePath" ).Value );
                Assert.AreEqual( @"c:\myapp\", document.Root.Element( "PhysicalApplicationPath" ).Value );
                Assert.AreEqual( "~/foo/bar", document.Root.Element( "AppRelativeCurrentExecutionFilePath" ).Value );

                Assert.AreEqual( "127.0.0.1", document.Root.Element( "ServerAddress" ).Value );
                Assert.AreEqual( "99.99.99.1", document.Root.Element( "ClientAddress" ).Value );
                Assert.AreEqual( "POST", document.Root.Element( "HttpMethod" ).Value );
                Assert.AreEqual( "false", document.Root.Element( "Https" ).Value );
                Assert.AreEqual( "localhost", document.Root.Element( "HostName" ).Value );
                Assert.AreEqual( "80", document.Root.Element( "ServerPort" ).Value );
                Assert.AreEqual( "http://localhost/foo/bar/", document.Root.Element( "RawUrl" ).Value );

                foreach( string key in exceptionContextBuilder.HttpRequestMocker.ServerVariables.AllKeys )
                {
                    Assert.AreEqual( exceptionContextBuilder.HttpRequestMocker.ServerVariables[ key ],
                                     document.Root.Element( "ServerVariables" ).Elements().Where( el => el.Name == key )
                                         .First().Value );
                }

                foreach( string key in exceptionContextBuilder.HttpRequestMocker.Headers.AllKeys )
                {
                    Assert.AreEqual( exceptionContextBuilder.HttpRequestMocker.Headers[ key ],
                                     document.Root.Element( "RequestHeaders" ).Elements().Where( el => el.Name == key )
                                         .First().Value );
                }

                Assert.AreEqual( "My request body", document.Root.Element( "RequestBody" ).Value );
            }
            finally
            {
                Trace.Listeners.Remove( traceListener );
            }
        }

        private ExceptionContextBuilder _getExceptionContextBuilder( Exception exception )
        {
            HttpRequestMocker requestMocker = new HttpRequestMocker()
                                                  {
                                                      ServerVariables = new NameValueCollection
                                                                            {
                                                                                {"SERVER_NAME", "localhost"},
                                                                                {"SERVER_PORT", "80"},
                                                                                {"HTTPS", "off"},
                                                                                {"HTTP_HOST", "localhost"},
                                                                                {"REMOTE_ADDR", "99.99.99.1"},
                                                                                {"REMOTE_HOST", "THEIR_CLIENT"},
                                                                                {"LOCAL_ADDR", "127.0.0.1"},
                                                                                {"LOCAL_HOST", "MY_SERVER"}
                                                                            },
                                                      Headers = new NameValueCollection
                                                                    {
                                                                        {"Rest-User", "SomeUser"}
                                                                    },
                                                      Url = new Uri( "http://localhost/foo/bar/" ),
                                                      ApplicationPath = "/",
                                                      Path = "/foo/bar",
                                                      FilePath = "/foo/bar",
                                                      CurrentExecutionFilePath = "/foo/bar",
                                                      PhysicalApplicationPath = @"c:\myapp\",
                                                      PathInfo = string.Empty,
                                                      AppRelativeCurrentExecutionFilePath = "~/foo/bar",
                                                      HttpMethod = "POST",
                                                      RequestBody = "My request body"
                                                  };

            HttpContextMocker contextMocker = new HttpContextMocker();

            MvcControllerMocker controllerMocker = new MvcControllerMocker()
                                                       {
                                                           Name = "MyController",
                                                           Action = "MyAction",
                                                           RouteUrl = "{controller}/{action}/{id}"
                                                       };

            ExceptionContextBuilder contextBuilder = new ExceptionContextBuilder();
            ExceptionContext exceptionContext = contextBuilder.Build( exception, requestMocker, contextMocker,
                                                                      controllerMocker );

            return contextBuilder;
        }
    }
}