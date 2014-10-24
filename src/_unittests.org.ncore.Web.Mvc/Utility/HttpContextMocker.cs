using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Moq;

namespace _unittests.org.ncore.Web.Mvc.Utility
{
    public class HttpContextMocker
    {
        public HttpRequestBase Request { get; set; }
        public Mock<HttpContextBase> Mock { get; private set; }

        public Mock<HttpContextBase> BuildMock()
        {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>( MockBehavior.Strict );
            mockHttpContext.Setup( c => c.Request ).Returns( this.Request );
            mockHttpContext.Setup( c => c.IsCustomErrorEnabled ).Returns( true );
            mockHttpContext.Setup( c => c.Session ).Returns( (HttpSessionStateBase)null );
            mockHttpContext.Setup( c => c.Response.Clear() ).Verifiable();
            mockHttpContext.Setup( c => c.Response.ClearContent() ).Verifiable();
            mockHttpContext.SetupSet( c => c.Response.StatusCode = 400 ).Verifiable();
            mockHttpContext.SetupSet( c => c.Response.TrySkipIisCustomErrors = true ).Verifiable();

            this.Mock = mockHttpContext;
            return mockHttpContext;
        }
    }
}
