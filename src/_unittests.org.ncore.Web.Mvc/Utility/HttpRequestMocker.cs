using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Moq;

namespace _unittests.org.ncore.Web.Mvc.Utility
{
    public class HttpRequestMocker
    {
        public NameValueCollection Headers { get; set; }
        public NameValueCollection ServerVariables { get; set; }
        public NameValueCollection QueryString { get; set; }
        public Uri Url { get; set; }
        public string ApplicationPath { get; set; }
        public string Path { get; set; }
        public string FilePath { get; set; }
        public string CurrentExecutionFilePath { get; set; }
        public string PhysicalApplicationPath { get; set; }
        public string PathInfo { get; set; }
        public string AppRelativeCurrentExecutionFilePath { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBody { get; set; }
        public Encoding ContentEncoding { get; set; }
        public Mock<HttpRequestBase> Mock { get; private set; }

        public HttpRequestMocker()
        {
            Headers = new NameValueCollection();
            ServerVariables = new NameValueCollection();
            QueryString = new NameValueCollection();
            ContentEncoding = Encoding.UTF8;
        }

        public Mock<HttpRequestBase> BuildMock()
        {
            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup( x => x.Headers ).Returns( this.Headers );
            mockRequest.Setup( x => x.ServerVariables ).Returns( this.ServerVariables );
            mockRequest.Setup( x => x.QueryString ).Returns( this.QueryString );
            mockRequest.Setup( x => x.Url ).Returns( this.Url );
            mockRequest.Setup( x => x.RawUrl ).Returns( this.Url.ToString() );
            mockRequest.Setup( x => x.ApplicationPath ).Returns( this.ApplicationPath );
            mockRequest.Setup( x => x.Path ).Returns( this.Path );
            mockRequest.Setup( x => x.FilePath ).Returns( this.FilePath );
            mockRequest.Setup( x => x.CurrentExecutionFilePath ).Returns( this.CurrentExecutionFilePath );
            mockRequest.Setup( x => x.PhysicalApplicationPath ).Returns( this.PhysicalApplicationPath );
            mockRequest.Setup( x => x.PathInfo ).Returns( this.PathInfo );
            mockRequest.Setup( x => x.AppRelativeCurrentExecutionFilePath ).Returns(
                this.AppRelativeCurrentExecutionFilePath );
            mockRequest.Setup( c => c.HttpMethod ).Returns( this.HttpMethod );
            mockRequest.Setup( c => c.ContentEncoding ).Returns( this.ContentEncoding );


            MemoryStream inputStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter( inputStream );
            writer.Write( this.ContentEncoding.GetBytes( this.RequestBody ) );
            mockRequest.Setup( x => x.InputStream ).Returns( inputStream );
            mockRequest.Setup( x => x.BinaryRead( (int)inputStream.Length ) ).Returns(
                this.ContentEncoding.GetBytes( this.RequestBody ) );
            // TODO: Why doesn't MOQ support .Returns( long )?!  The type of Stream.Length is a long!  JF
            mockRequest.Setup( x => x.TotalBytes ).Returns( (int)inputStream.Length );

            this.Mock = mockRequest;
            return mockRequest;
        }
    }
}
