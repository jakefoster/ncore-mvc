using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace org.ncore.Web.Mvc.Services
{
    public interface IWebFaultLogger
    {
        void Initialize( ExceptionContext filterContext, XmlFault fault );
        void LogFault();
    }
}
