﻿using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Jajo.Web.Endpoints
{
    public class ExecuteZeroModelInZeroModelOutEndpoint<TEndpoint> : IExecuteEndpoint
    {
        private readonly TEndpoint _endpoint;

        public ExecuteZeroModelInZeroModelOutEndpoint(TEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Execute(MethodInfo endpointMethod, IDictionary<string, object> environment)
        {
            if (endpointMethod.IsAsyncMethod())
                await (Task)endpointMethod.Invoke(_endpoint, new object[0]);
            else
                endpointMethod.Invoke(_endpoint, new object[0]);
        }
    }
}