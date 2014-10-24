using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.ncore.Common;
using org.ncore.Extensions;

namespace _unittests.org.ncore.Web.Mvc
{
    public class UnitTestTraceListener : TraceListener
    {
        private StringBuilder _target;

        public UnitTestTraceListener( StringBuilder target )
        {
            _target = target;
        }

        public override void Write( string message )
        {
            WriteIndent();
            _target.Append( message );
        }

        protected override void WriteIndent()
        {
            _target.Append( string.Empty.Fill( " ", ( this.IndentLevel * this.IndentSize ) ) );
        }

        public override void WriteLine( string message )
        {
            WriteIndent();
            _target.AppendLine( message );
        }
    }
}
