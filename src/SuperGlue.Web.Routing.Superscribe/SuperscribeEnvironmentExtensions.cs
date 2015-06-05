﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Superscribe.Engine;
using Superscribe.Models;

namespace SuperGlue.Web.Routing.Superscribe
{
    public static class SuperscribeEnvironmentExtensions
    {
        public static class SuperscribeConstants
        {
            public const string Engine = "superglue.Superscribe.Engine";
            public const string EndpointToRouteList = "superglue.Superscribe.EndpointToRouteList";
        }

        public static IRouteEngine GetRouteEngine(this IDictionary<string, object> environment)
        {
            return environment.Get<IRouteEngine>(SuperscribeConstants.Engine);
        }

        public static EndpointRoute GetRouteForEndpoint(this IDictionary<string, object> environment, object endpoint)
        {
            if (endpoint == null)
                return null;

            var endpointRoutes = environment.Get<IDictionary<object, Tuple<ICollection<GraphNode>, IDictionary<Type, Func<object, IDictionary<string, object>>>>>>(SuperscribeConstants.EndpointToRouteList,
                new Dictionary<object, Tuple<ICollection<GraphNode>, IDictionary<Type, Func<object, IDictionary<string, object>>>>>());

            if (endpointRoutes.ContainsKey(endpoint))
            {
                var node = endpointRoutes[endpoint].Item1.FirstOrDefault();

                return node != null ? new EndpointRoute(node) : null;
            }

            return endpointRoutes
                .Where(x => x.Value.Item2.ContainsKey(endpoint.GetType()) && x.Value.Item1.Any())
                .Select(x => new EndpointRoute(x.Value.Item1.First(), x.Value.Item2[endpoint.GetType()](endpoint)))
                .FirstOrDefault();
        }

        internal static void AddRouteToEndpoint(this IDictionary<string, object> environment, object endpoint, IDictionary<Type, Func<object, IDictionary<string, object>>> routedInputs, GraphNode route)
        {
            var endpointRoutes = environment.Get<IDictionary<object, Tuple<ICollection<GraphNode>, IDictionary<Type, Func<object, IDictionary<string, object>>>>>>(SuperscribeConstants.EndpointToRouteList);

            if (endpointRoutes == null)
            {
                endpointRoutes = new ConcurrentDictionary<object, Tuple<ICollection<GraphNode>, IDictionary<Type, Func<object, IDictionary<string, object>>>>>();
                environment[SuperscribeConstants.EndpointToRouteList] = endpointRoutes;
            }

            if(!endpointRoutes.ContainsKey(endpoint))
                endpointRoutes[endpoint] = new Tuple<ICollection<GraphNode>, IDictionary<Type, Func<object, IDictionary<string, object>>>>(new List<GraphNode>(), new Dictionary<Type, Func<object, IDictionary<string, object>>>());

            endpointRoutes[endpoint].Item1.Add(route);

            foreach (var routedInput in routedInputs)
                endpointRoutes[endpoint].Item2[routedInput.Key] = routedInput.Value;
        }

        public class EndpointRoute
        {
            public EndpointRoute(GraphNode node, IDictionary<string, object> parameters = null)
            {
                Node = node;
                Parameters = parameters ?? new Dictionary<string, object>();
            }

            public GraphNode Node { get; private set; }
            public IDictionary<string, object> Parameters { get; private set; }
        }
    }
}