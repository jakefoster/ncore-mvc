using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace org.ncore.Web.Mvc.Services
{
    public interface IRestFaultException
    {
        int StatusCode { get; set;  }
        int SubStatusCode { get; set;  }
        string StatusDescription { get; set; }
        NameValueCollection CustomHeaders { get; set; }
    }
}
