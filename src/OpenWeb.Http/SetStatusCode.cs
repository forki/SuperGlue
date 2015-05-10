﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenWeb.Http
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetStatusCode
    {
        private readonly AppFunc _next;
        private readonly int _statusCode;

        public SetStatusCode(AppFunc next, int statusCode)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _statusCode = statusCode;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = _statusCode;

            await _next(environment);
        }
    }
}