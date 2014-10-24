using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace _unittests.org.ncore.Web.Mvc.Utility
{
    public class MvcControllerMocker
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public string RouteUrl { get; set; }
        public RouteData RouteData { get; set; }
        public Mock<Controller> Mock { get; private set; }

        public Mock<Controller> BuildMock()
        {
            // CONTROLLER
            TempDataDictionary tempData = new TempDataDictionary();
            Mock<Controller> controller = new Mock<Controller>();
            controller.Object.TempData = tempData;

            // ROUTEDATA
            this.RouteData = new RouteData();
            this.RouteData.Values[ "controller" ] = this.Name;
            this.RouteData.Values[ "action" ] = this.Action;

            MvcRouteHandler routeHandler = new MvcRouteHandler();

            Route route = new Route( this.RouteUrl, routeHandler );
            this.RouteData.Route = route;

            this.Mock = controller;
            return controller;
        }
    }
}
