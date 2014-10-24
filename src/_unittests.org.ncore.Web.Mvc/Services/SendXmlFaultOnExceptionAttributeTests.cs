using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _unittests.org.ncore.Web.Mvc.Utility;
using org.ncore.Exceptions;
using org.ncore.Web.Mvc.Services;

namespace _unittests.org.ncore.Web.Mvc.Services
{
    /// <summary>
    /// Summary description for SendXmlFaultOnExceptionAttributeTests
    /// </summary>
    [TestClass]
    public class SendXmlFaultOnExceptionAttributeTests
    {
        public SendXmlFaultOnExceptionAttributeTests()
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

        // TODO: Need to refine these tests.  JF
        [TestMethod]
        public void ThrowApplicationException()
        {
            // ARRANGE
            ExceptionContextBuilder exceptionContextBuilder = null;
            try
            {
                throw new ApplicationException( "Something bad happened." );
            }
            catch( Exception ex )
            {
                exceptionContextBuilder = _getExceptionContextBuilder( ex );
            }

            // ACT
            SendXmlFaultOnExceptionAttribute attribute = new SendXmlFaultOnExceptionAttribute();
            attribute.OnException( exceptionContextBuilder.ExceptionContext );

            // ASSERT
            ExceptionContext context = exceptionContextBuilder.ExceptionContext;
            Assert.AreEqual( 400, ( (XmlResult)context.Result ).StatusCode );
            Assert.AreEqual( 0, ( (XmlResult)context.Result ).SubStatusCode );
            Assert.AreEqual( "BadRequest", ( (XmlResult)context.Result ).StatusDescription );
            // TODO: Assert header contents here.  JF
            Assert.AreEqual( 1, ( (XmlResult)context.Result ).Headers.Count );
            // TODO: Assert content of Xml?  Or does this belon over in XmlFault only?  JF
        }

        [TestMethod]
        public void ThrowIRestFaultException()
        {
            // ARRANGE
            ExceptionContextBuilder exceptionContextBuilder = null;
            try
            {
                RestException exception = new RestException()
                                              {
                                                  StatusCode = 401,
                                                  SubStatusCode = 1,
                                                  StatusDescription = "BAD THINGS HAPPENED",
                                                  CustomHeaders = new NameValueCollection()
                                                                      {
                                                                          {"Thing1", "a"},
                                                                          {"Thing2", "b"}
                                                                      }
                                              };
                throw exception;
            }
            catch( Exception ex )
            {
                exceptionContextBuilder = _getExceptionContextBuilder( ex );
            }

            // ACT
            SendXmlFaultOnExceptionAttribute attribute = new SendXmlFaultOnExceptionAttribute();
            attribute.OnException( exceptionContextBuilder.ExceptionContext );

            // ASSERT
            ExceptionContext context = exceptionContextBuilder.ExceptionContext;
            Assert.AreEqual( 401, ( (XmlResult)context.Result ).StatusCode );
            Assert.AreEqual( 1, ( (XmlResult)context.Result ).SubStatusCode );
            Assert.AreEqual( "BAD THINGS HAPPENED", ( (XmlResult)context.Result ).StatusDescription );
            // TODO: Assert header contents here.  JF
            Assert.AreEqual( 3, ( (XmlResult)context.Result ).Headers.Count );
            // TODO: Assert content of Xml?  Or does this belon over in XmlFault only?  JF
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
            contextBuilder.Build( exception, requestMocker, contextMocker, controllerMocker );

            return contextBuilder;
        }
    }

    public class RestException : ApplicationException, IRestFaultException
    {
        #region IRestFaultException Members

        private int _statusCode = 0;
        public int StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        private int _subStatusCode = 0;
        public int SubStatusCode
        {
            get { return _subStatusCode; }
            set { _subStatusCode = value; }
        }

        private string _statusDescription = string.Empty;
        public string StatusDescription
        {
            get { return _statusDescription; }
            set { _statusDescription = value; }
        }

        private NameValueCollection _customHeaders = new NameValueCollection();
        public NameValueCollection CustomHeaders
        {
            get { return _customHeaders; }
            set { _customHeaders = value; }
        }
        #endregion

        public RestException()
            : base( "A REST exception occurred." )
        {
        }
    }
}
