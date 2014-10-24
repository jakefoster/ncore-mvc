using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace _unittests.org.ncore.Web.Mvc.Utility
{
    public class ControllerContextBuilder
    {
        public HttpRequestMocker HttpRequestMocker { get; private set; }
        public HttpContextMocker ContextMocker { get; private set; }
        public MvcControllerMocker ControllerMocker { get; private set; }
        public RequestContext RequestContext { get; private set; }
        public ControllerContext ControllerContext { get; private set; }

        public ControllerContext Build( HttpRequestMocker requestMocker, HttpContextMocker contextMocker, MvcControllerMocker controllerMocker )
        {
            this.HttpRequestMocker = requestMocker;

            ContextMocker = contextMocker;
            ContextMocker.Request = this.HttpRequestMocker.BuildMock().Object;

            Mock<HttpContextBase> mockContext = this.ContextMocker.BuildMock();

            ControllerMocker = controllerMocker;

            Mock<Controller> mockController = this.ControllerMocker.BuildMock();

            this.RequestContext = new RequestContext( mockContext.Object, this.ControllerMocker.RouteData );
            this.ControllerContext = new ControllerContext( this.RequestContext, mockController.Object );

            return this.ControllerContext;
        }
    }
}
