﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.FeatureToggler
{
    public class SetupFeatureConfiguration : ISetupConfigurations
    {
        public IEnumerable<ConfigurationSetupResult> Setup(string applicationEnvironment)
        {
            yield return new ConfigurationSetupResult("superglue.FeaturesSetup", environment =>
            {
                environment.RegisterTransient(typeof(ICheckIfFeatureIsEnabled), typeof(DefaultFeatureEnabledChecker));

                return Task.CompletedTask;
            }, "superglue.ContainerSetup");
        }
    }
}