using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using org.ncore.Diagnostics;
using org.ncore.Exceptions;
using org.ncore.Extensions;
using org.ncore.Web.Mvc.Services;
using System.Xml.Linq;
using System.Web.Routing;

namespace org.ncore.Web.Mvc.Services
{
    // NOTE: While useful this class is really intended more to be an example of
    //  the IWebFaultLogger pattern and how a WebFaultLogger could be implemented.
    //  A more useful implementation might write to a SQL DB or the Event Log.  JF
    public class WebFaultTraceLogger : IWebFaultLogger
    {
        private XDocument _webFault;

        public void Initialize( ExceptionContext filterContext, XmlFault fault )
        {
            if( _webFault == null )
            {
                Spy.Trace( "In WebServiceFaultLogger constructor" );
                try
                {
                    Exception exception = filterContext.Exception;
                    HttpRequestBase request = filterContext.RequestContext.HttpContext.Request;

                    _webFault = new XDocument( new XElement( "WebFault" ) );
                    _webFault.Root.Add( fault.BuildXml( true ).Root );
                    _webFault.Root.Add( new XElement( "ServerName", Environment.MachineName ) );
                    _webFault.Root.Add( new XElement( "ServiceName",
                                                          String.Format( "{0}.{1}",
                                                                         filterContext.RouteData.Values[ "controller" ],
                                                                         filterContext.RouteData.Values[ "action" ] ) ) );
                    _webFault.Root.Add( new XElement( "RouteUrl", ( (Route)filterContext.RouteData.Route ).Url ) );
                    _webFault.Root.Add( new XElement( "Path", request.Path ) );
                    _webFault.Root.Add( new XElement( "FilePath", request.FilePath ) );
                    _webFault.Root.Add( new XElement( "ApplicationPath", request.ApplicationPath ) );
                    _webFault.Root.Add( new XElement( "CurrentExecutionFilePath", request.CurrentExecutionFilePath ) );
                    _webFault.Root.Add( new XElement( "PhysicalApplicationPath", request.PhysicalApplicationPath ) );
                    _webFault.Root.Add( new XElement( "AppRelativeCurrentExecutionFilePath", request.AppRelativeCurrentExecutionFilePath ) );
                    
                    _webFault.Root.Add( new XElement( "ServerAddress", request.ServerVariables[ "LOCAL_ADDR" ] ) );
                    _webFault.Root.Add( new XElement( "ClientAddress", request.ServerVariables[ "REMOTE_ADDR" ] ) );

                    _webFault.Root.Add( new XElement( "HttpMethod", request.HttpMethod ) );
                    _webFault.Root.Add( new XElement( "Https", request.ServerVariables[ "HTTPS" ].ToLower() == "on" ? true : false ) );
                    _webFault.Root.Add( new XElement( "HostName", request.ServerVariables[ "HTTP_HOST" ] ) );
                    _webFault.Root.Add( new XElement( "ServerPort", int.Parse( request.ServerVariables[ "SERVER_PORT" ] ) ) );
                    _webFault.Root.Add( new XElement( "RawUrl", request.RawUrl ) );

                    XElement serverVariables = new XElement( "ServerVariables" );
                    foreach( string key in request.ServerVariables.Keys )
                    {
                        serverVariables.Add( new XElement( key, new XText( HttpUtility.HtmlEncode( request.ServerVariables[ key ] ) ) ) );
                    }
                    _webFault.Root.Add( serverVariables );

                    XElement requestHeaders = new XElement( "RequestHeaders" );
                    foreach( string key in request.Headers.Keys )
                    {
                        requestHeaders.Add( new XElement( key, new XText( HttpUtility.HtmlEncode( request.Headers[ key ] ) ) ) );
                    }
                    _webFault.Root.Add( requestHeaders );

                    request.InputStream.Seek( 0, System.IO.SeekOrigin.Begin );
                    string requestBody = request.BinaryRead( request.TotalBytes ).ToText( request.ContentEncoding );
                    _webFault.Root.Add( new XElement( "RequestBody" , requestBody ) );
                }
                catch( Exception ex )
                {
                    Spy.Trace( ex );
                    throw;
                }
            }
            else
            {
                Spy.Trace( "Initialize() was called but this instance has already been initialized." );
            }
        }

        public void LogFault()
        {
            if( _webFault != null )
            {
                try
                {
                    Spy.Trace( _webFault );
                    _webFault = null;
                }
                catch( Exception ex )
                {
                    Spy.Trace( ex );
                    throw;
                }
            }
            else
            {
                Spy.Trace( "LogFault() was called but there was no WebFault to log.  This is most likely because the instance was not initialized or LogFault() was already called." );
            }
        }
    }
}