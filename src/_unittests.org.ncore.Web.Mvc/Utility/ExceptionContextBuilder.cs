using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace _unittests.org.ncore.Web.Mvc.Utility
{
    public class ExceptionContextBuilder
    {
        public HttpRequestMocker HttpRequestMocker { get; private set; }
        public HttpContextMocker ContextMocker { get; private set; }
        public MvcControllerMocker ControllerMocker { get; private set; }
        public RequestContext RequestContext { get; private set; }
        public ControllerContext ControllerContext { get; private set; }
        public ExceptionContext ExceptionContext { get; private set; }
        public Exception Exception { get; private set; }

        public ExceptionContext Build( Exception exception, HttpRequestMocker requestMocker, HttpContextMocker contextMocker, MvcControllerMocker controllerMocker )
        {
            this.Exception = exception;

            this.HttpRequestMocker = requestMocker;

            ContextMocker = contextMocker;
            ContextMocker.Request = this.HttpRequestMocker.BuildMock().Object;

            Mock<HttpContextBase> mockContext = this.ContextMocker.BuildMock();

            ControllerMocker = controllerMocker;

            Mock<Controller> mockController = this.ControllerMocker.BuildMock();

            this.RequestContext = new RequestContext( mockContext.Object, this.ControllerMocker.RouteData );
            this.ControllerContext = new ControllerContext( this.RequestContext, mockController.Object );
            this.ExceptionContext = new ExceptionContext( this.ControllerContext, exception );

            return this.ExceptionContext;
        }
    }
}
