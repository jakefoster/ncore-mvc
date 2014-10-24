using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using org.ncore.Diagnostics;
using org.ncore.Extensions;
using org.ncore.Exceptions;

namespace org.ncore.Web.Mvc.Services
{
    public class XmlFault
    {
        public static class Configuration
        {
            public static bool? Verbose
            {
                get
                {
                    const string settingName = "org.ncore.Web.Mvc/Services/XmlFault.Verbose";
                    if( ConfigurationManager.AppSettings[ settingName ] != null )
                    {
                        return Boolean.Parse( ConfigurationManager.AppSettings[ settingName ] );
                    }
                    return null;
                }
            }
        }

        private const string SCHEMA_REVISION = "1";

        public XNamespace Namespace = "http://schemas.ncore.org/Web/Mvc/Service/Fault/1/";
        public Exception Exception { get; set; }
        public string Key { get; set; }
        public string Action { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public bool? Verbose { get; set; }

        public XmlFault()
        {
            this.Key = string.Empty;
            this.Action = string.Empty;
        }

        public XDocument BuildXml()
        {
            return BuildXml( null );  
        }

        public XDocument BuildXml( bool? verbose )
        {
            return _transformExceptionToFault( Namespace, this.Action, this.Key, this.Exception, this.Metadata, _determineVerbosity( verbose ) );
        }

        private bool _determineVerbosity( bool? verbose )
        {
            if( verbose == null )
            {
                if( this.Verbose != null )
                {
                    return this.Verbose.Value;
                }
                else if( XmlFault.Configuration.Verbose != null )
                {
                    return XmlFault.Configuration.Verbose.Value;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return verbose.Value;
            }
        }

        private static XDocument _transformExceptionToFault( XNamespace @namespace, string action, string key, Exception exception, Dictionary<string, string> metadata, bool verbose )
        {

            XDocument fault = new XDocument(
                new XElement( @namespace + "Fault",
                    new XAttribute( "Key", key ),
                    new XAttribute( "Action", action ),
                    new XAttribute( "Revision", SCHEMA_REVISION ),
                    _renderExceptionXml( @namespace, exception, verbose ),
                    new XElement( @namespace + "Metadata" )
                    ) );

            foreach( string itemKey in ( (Dictionary<string, string>)metadata.DefaultIfNull( new Dictionary<string, string>() ) ).Keys )
            {
                fault.Root.Element( @namespace + "Metadata" ).Add(
                    new XElement( @namespace + "Item",
                        new XAttribute( "Type", itemKey ),
                        new XText( metadata[ itemKey ] ) ) );
            }

            return fault;
        }

        private static XElement _renderExceptionXml( XNamespace @namespace, Exception exception, bool verbose )
        {
            if( exception == null )
            {
                return null;
            }

            string instructions = string.Empty;
            if( exception is BaseException )
            {
                instructions = ( (BaseException)exception ).Instructions;
            }

            XElement innerExceptionXml = exception.InnerException != null ? _renderExceptionXml( @namespace, exception.InnerException, verbose ) : null;
            
            XElement element = null;
            if( verbose )
            {
                element = new XElement(
                    new XElement( @namespace + "Exception",
                        new XElement( @namespace + "Type",
                            new XText( exception.GetType().FullName ) ),
                        new XElement( @namespace + "Message",
                            new XText( exception.Message ) ),
                        new XElement( @namespace + "Instructions",
                            new XText( instructions ) ) ,
                        new XElement( @namespace + "Source",
                            new XText( exception.Source ?? string.Empty ) ),
                        new XElement( @namespace + "Operation",
                            new XText( exception.TargetSite != null ? exception.TargetSite.ToString() : string.Empty ) ),
                        new XElement( @namespace + "StackTrace",
                            new XText( exception.StackTrace != null ? exception.StackTrace.ToString() : string.Empty ) ),
                        new XElement( @namespace + "InnerException",
                            innerExceptionXml ) ) );
            }
            else
            {
                element = new XElement(
                    new XElement( @namespace + "Exception",
                        new XElement( @namespace + "Type",
                            new XText( exception.GetType().ToString() ) ),
                        new XElement( @namespace + "Instructions",
                            new XText( instructions ) ),
                        new XElement( @namespace + "InnerException",
                            innerExceptionXml ) ) );
            }

            return element;
        }
    }
}