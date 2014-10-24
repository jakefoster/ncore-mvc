using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using org.ncore.Extensions;

namespace org.ncore.Web.Mvc.Services
{
    // TODO: Unit tests!  JF
    public class XmlResult : ActionResult
    {
        
        private int _statusCode = 200;
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

        private string _statusDescription = "OK";
        public string StatusDescription
        {
            get { return _statusDescription; }
            set { _statusDescription = value; }
        }

        private string _contentType = "text/xml";
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        private NameValueCollection _headers = new NameValueCollection();
        public NameValueCollection Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        private XDocument _responseXml = null;
        public XDocument ResponseXml
        {
            get { return _responseXml; }
            set { _responseXml = value; }
        }

        private Encoding _encoding = Encoding.UTF8;
        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        // TODO: At some point the Obsolete constructors should be removed. JF
        public XmlResult()
        {
        }

        // TODO: Not sure if this should be obsoleted.  This is basically a "required" property.  
        //  But if so then shouldn't the default constructor be removed and the property setter 
        //  made private?  JF
        public XmlResult( XDocument responseXml )
        {
            _responseXml = responseXml;
        }

        [Obsolete("Obsolete as of version 1.7.  Use construction initializers or properties instead.")]
        public XmlResult( XDocument responseXml, int statusCode, string statusDescription )
        {
            _responseXml = responseXml;
            _statusCode = statusCode;
            _statusDescription = statusDescription;
        }

        [Obsolete( "Obsolete as of version 1.7.  Use construction initializers or properties instead." )]
        public XmlResult( XDocument responseXml, NameValueCollection headers )
        {
            _responseXml = responseXml;
            _headers = headers;
        }

        [Obsolete( "Obsolete as of version 1.7.  Use construction initializers or properties instead." )]
        public XmlResult( XDocument responseXml, int statusCode, string statusDescription, NameValueCollection headers )
        {
            _responseXml = responseXml;
            _statusCode = statusCode;
            _statusDescription = statusDescription;
            _headers = headers;
        }

        [Obsolete( "Obsolete as of version 1.7.  Use construction initializers or properties instead." )]
        public XmlResult( XDocument responseXml, Encoding encoding )
        {
            _responseXml = responseXml;
            _encoding = encoding;
        }

        [Obsolete( "Obsolete as of version 1.7.  Use construction initializers or properties instead." )]
        public XmlResult( XDocument responseXml, int statusCode, string statusDescription, Encoding encoding )
        {
            _responseXml = responseXml;
            _statusCode = statusCode;
            _statusDescription = statusDescription;
            _encoding = encoding;
        }

        [Obsolete( "Obsolete as of version 1.7.  Use construction initializers or properties instead." )]
        public XmlResult( XDocument responseXml, NameValueCollection headers, Encoding encoding )
        {
            _responseXml = responseXml;
            _headers = headers;
            _encoding = encoding;
        }

        [Obsolete( "Obsolete as of version 1.7.  Use construction initializers or properties instead." )]
        public XmlResult( XDocument responseXml, int statusCode, string statusDescription, NameValueCollection headers, Encoding encoding )
        {
            _responseXml = responseXml;
            _statusCode = statusCode;
            _statusDescription = statusDescription;
            _headers = headers;
            _encoding = encoding;
        }

        public override void ExecuteResult( ControllerContext context )
        {
            HttpResponseBase response = context.HttpContext.Response;
            response.ClearContent();
            response.StatusCode = _statusCode;
            response.SubStatusCode = _subStatusCode;
            response.StatusDescription = _statusDescription;
            if( _headers != null )
            {
                foreach( string key in _headers.AllKeys )
                {
                    response.AddHeader( key, _headers[ key ] );
                }
            }
            response.ContentType = _contentType;

            // NOTE: Override the Encoding property if the actual document declaration specifies the encoding.
            //  This could be confusing if the caller has explicitly set the Encoding property, but I do believe 
            //  that it's the right thing to do nonetheless.  It's either this or throw.  JF
            if( _responseXml.Declaration != null && _responseXml.Declaration.Encoding != null && _responseXml.Declaration.Encoding != string.Empty )
            {
                _encoding = Encoding.GetEncoding( _responseXml.Declaration.Encoding, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback );
            }
            response.ContentEncoding = _encoding;

            XmlWriterSettings settings = new XmlWriterSettings()
                                             {
                                                 Encoding = _encoding,
                                                 NewLineHandling = NewLineHandling.Entitize,
                                                 Indent = true
                                             };

            response.Write( _responseXml.ToText( settings ) );
            response.End();
        }
    }
}
