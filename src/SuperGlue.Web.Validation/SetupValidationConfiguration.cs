﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Web.Validation.InputValidation;

namespace SuperGlue.Web.Validation
{
    public class SetupValidationConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.ValidationSetup", environment =>
            {
                environment.RegisterAllClosing(typeof(IValidateInput<>));
                environment.RegisterAll(typeof(IValidateRequest));
            }, "superglue.Container");
        }

        public Task Shutdown(IDictionary<string, object> applicationData)
        {
            throw new System.NotImplementedException();
        }

        public Task Configure(SettingsConfiguration configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}