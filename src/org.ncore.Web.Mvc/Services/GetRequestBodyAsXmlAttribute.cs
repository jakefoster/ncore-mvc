using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using org.ncore.Extensions;

namespace org.ncore.Web.Mvc.Services
{
    public class GetRequestBodyAsXmlAttribute : ActionFilterAttribute
    {
        public GetRequestBodyAsXmlAttribute()
        {
        }

        public override void OnActionExecuting( ActionExecutingContext filterContext )
        {
            HttpRequestBase request = filterContext.RequestContext.HttpContext.Request;

            int byteCount = request.TotalBytes;
            string requestBody = string.Empty;
            if( byteCount > 0 )
            {
                request.InputStream.Seek( 0, System.IO.SeekOrigin.Begin );
                requestBody = request.BinaryRead( byteCount ).ToText( request.ContentEncoding );
            }

            XDocument document = XDocument.Parse( requestBody );

            //filterContext.Controller.ViewData.Model = document;
            ( (dynamic) filterContext.Controller ).RequestXml = document;

            base.OnActionExecuting( filterContext );
        }
    }
}