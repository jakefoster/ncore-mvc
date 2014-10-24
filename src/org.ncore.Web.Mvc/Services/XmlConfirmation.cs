using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using org.ncore.Diagnostics;
using org.ncore.Extensions;
using org.ncore.Exceptions;

namespace org.ncore.Web.Mvc.Services
{
    public class XmlConfirmation
    {
        private const string SCHEMA_REVISION = "1";

        public XNamespace Namespace = "http://schemas.ncore.org/Web/Mvc/Service/Confirmation/1/";
        public string Key { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public XmlConfirmation()
        {
            this.Key = string.Empty;
            this.Action = string.Empty;
            this.Message = string.Empty;
        }

        public XDocument BuildXml()
        {
            return _buildXml( this.Namespace, this.Action, this.Key, this.Message, this.Metadata );
        }

        private static XDocument _buildXml( XNamespace @namespace, string action, string key, string message, Dictionary<string, string> metadata )
        {
            XDocument confirmation = new XDocument(
                                        new XElement( @namespace + "Confirmation",
                                            new XAttribute( "Action", action ),
                                            new XAttribute( "Key", key ),
                                            new XAttribute( "Revision", SCHEMA_REVISION ),
                                            new XElement( @namespace + "Message",
                                                new XText( message ) ),
                                            new XElement( @namespace + "Metadata" ) ) );

            foreach( string itemKey in ( (Dictionary<string, string>)metadata.DefaultIfNull( new Dictionary<string, string>() ) ).Keys )
            {
                confirmation.Root.Element( @namespace + "Metadata" ).Add(
                    new XElement( @namespace + "Item",
                        new XAttribute( "Type", itemKey ),
                        new XText( metadata[ itemKey ] ) ) );
            }

            return confirmation;
        }
    }
}