using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using org.ncore.Common;
using org.ncore.Diagnostics;
using org.ncore.Extensions;
using org.ncore.Exceptions;
using org.ncore.Web.Mvc;

namespace org.ncore.Web.Mvc.Services
{
    public class SendXmlFaultOnExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        private readonly IWebFaultLogger _logger;

        private int _httpStatusCode = 400;
        public int HttpStatusCode
        {
            get { return _httpStatusCode; }
            set { _httpStatusCode = value; }
        }

        private int _httpSubStatusCode = 0;
        public int HttpSubStatusCode
        {
            get { return _httpSubStatusCode; }
            set { _httpSubStatusCode = value; }
        }

        private NameValueCollection _customHttpHeaders = new NameValueCollection();
        public NameValueCollection CustomHttpHeaders
        {
            get { return _customHttpHeaders; }
            set { _customHttpHeaders = value; }
        }

        private string _httpStatusDescription = string.Empty;
        public string HttpStatusDescription
        {
            get { return _httpStatusDescription; }
            set { _httpStatusDescription = value; }
        }

        private string _faultKeyHeaderName = "REST-Fault-Key";
        public string FaultKeyHeaderName
        {
            get { return _faultKeyHeaderName; }
            set { _faultKeyHeaderName = value; }
        }

        public bool? _verbose;
        public bool Verbose
        {
            get
            {
                if( _verbose == null )
                {
                    string verboseSetting =
                        ConfigurationManager.AppSettings[ "org.ncore.Web.Mvc.Services/SendXmlFaultOnExceptionAttribute.Verbose"];
                    if( !verboseSetting.IsEmptyOrNull() )
                    {
                        bool parsed = false;
                        if( bool.TryParse( verboseSetting, out parsed ) )
                        {
                            _verbose = parsed;
                        }
                    }
                    else
                    {
                        _verbose = false;
                    }
                }
                return _verbose.Value;
            }

            set { _verbose = value; }
        }

        public string @Namespace { get; set; }

        public SendXmlFaultOnExceptionAttribute()
        {
        }

        public SendXmlFaultOnExceptionAttribute( bool verbose )
            : this( verbose, null )
        {
        }

        public SendXmlFaultOnExceptionAttribute( Type faultLoggerType )
            : this( null, faultLoggerType )
        {
        }

        public SendXmlFaultOnExceptionAttribute( bool verbose, Type faultLoggerType )
            : this( (bool?)verbose, faultLoggerType )
        {
        }

        private SendXmlFaultOnExceptionAttribute( bool? verbose, Type faultLoggerType )
        {
            try
            {
                _verbose = verbose;
                Condition.Assert( typeof( IWebFaultLogger ).IsAssignableFrom( faultLoggerType ) );
                _logger = (IWebFaultLogger)Activator.CreateInstance( faultLoggerType );
            }
            catch( Exception ex )
            {
                Spy.Trace( ex );
            }
        }

        public void OnException( ExceptionContext filterContext )
        {
            Exception exception = filterContext.Exception;
            Spy.Trace( exception );

            if( exception is IRestFaultException )
            {
                IRestFaultException restException = (IRestFaultException)exception;
                this.HttpStatusCode = restException.StatusCode;
                this.HttpSubStatusCode = restException.SubStatusCode;
                this.HttpStatusDescription = restException.StatusDescription;
                this.CustomHttpHeaders = restException.CustomHeaders;
            }
            
            if( this.HttpStatusDescription.IsEmptyOrNull() )
            {
                this.HttpStatusDescription = Enum.GetName( typeof(HttpStatusCode), this.HttpStatusCode );
            }

            string faultKey = _buildKey();
            Spy.Trace( "FaultKey: {0}", faultKey );

            XmlFault fault = new XmlFault()
                                 {
                                     Exception = exception, 
                                     Action = String.Format( "{0}.{1}", filterContext.RouteData.Values[ "controller" ], filterContext.RouteData.Values[ "action" ] ),
                                     Key = faultKey
                                 };

            if( this.Namespace != null )
            {
                fault.Namespace = this.Namespace;
            }

            XDocument faultXml = fault.BuildXml( this.Verbose );
            
            HttpRequestBase request = filterContext.RequestContext.HttpContext.Request;

            if( _logger != null )
            {
                try
                {
                    _logger.Initialize( filterContext, fault );
                    _logger.LogFault();
                }
                catch( Exception ex )
                {
                    Spy.Trace( ex );
                }
            }

            XmlResult result = new XmlResult( faultXml )
                                   {
                                       StatusCode = this.HttpStatusCode,
                                       SubStatusCode = this.HttpSubStatusCode,
                                       StatusDescription = this.HttpStatusDescription
                                   };
            if( this.FaultKeyHeaderName != null && _faultKeyHeaderName != string.Empty )
            {
                result.Headers.Add( "REST-Fault-Key", faultKey );
            }

            if( this.CustomHttpHeaders.Count > 0 )
            {
                foreach( string key in this.CustomHttpHeaders.Keys )
                {
                    result.Headers.Add( key, this.CustomHttpHeaders[ key ] );
                }
            }

            filterContext.Result = result;

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        private static string _buildKey()
        {
            return String.Format( "{0}_{1}", DateTime.UtcNow.ToString( "yyyy'.'MM'.'dd'_'HH'.'mm'.'ss" ),
                Guid.NewGuid().ToString().Substring( 0, 8 ).ToUpper() );
        }
    }
}