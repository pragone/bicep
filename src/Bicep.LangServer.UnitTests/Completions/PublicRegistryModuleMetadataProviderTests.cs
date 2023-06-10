// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Configuration;
using Bicep.Core.Diagnostics;
using Bicep.Core.Extensions;
using Bicep.Core.FileSystem;
using Bicep.Core.Modules;
using Bicep.Core.Registry;
using Bicep.Core.Syntax;
using Bicep.Core.Workspaces;
using Bicep.LangServer.IntegrationTests;
using Bicep.LanguageServer.CompilationManager;
using Bicep.LanguageServer.Providers;
using Bicep.LanguageServer.Registry;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bicep.LangServer.UnitTests.Completions
{
    [TestClass]
    public class PublicRegistryModuleMetadataProviderTests
    {
        private const string ModuleIndex = @"[
  {
    ""moduleName"": ""app/dapr-containerapp"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""app/dapr-containerapps-environment"",
    ""tags"": [
      ""1.0.1"",
      ""1.1.1"",
      ""1.2.1"",
      ""1.2.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""azure-gaming/game-dev-vm"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2"",
      ""2.0.1"",
      ""2.0.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""azure-gaming/game-dev-vmss"",
    ""tags"": [
      ""1.0.1"",
      ""1.1.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""compute/availability-set"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""compute/container-registry"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""compute/custom-image-vmss"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""cost/resourcegroup-scheduled-action"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""cost/subscription-scheduled-action"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""deployment-scripts/aks-run-command"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2"",
      ""1.0.3"",
      ""2.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""deployment-scripts/aks-run-helm"",
    ""tags"": [
      ""1.0.1"",
      ""2.0.1"",
      ""2.0.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""deployment-scripts/build-acr"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2"",
      ""2.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""deployment-scripts/create-kv-certificate"",
    ""tags"": [
      ""1.0.1"",
      ""1.1.1"",
      ""1.1.2"",
      ""2.1.1"",
      ""3.0.1"",
      ""3.0.2"",
      ""3.1.1"",
      ""3.2.1"",
      ""3.3.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""deployment-scripts/import-acr"",
    ""tags"": [
      ""1.0.1"",
      ""2.0.1"",
      ""2.1.1"",
      ""3.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""deployment-scripts/wait"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""identity/user-assigned-identity"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""lz/sub-vending"",
    ""tags"": [
      ""1.1.1"",
      ""1.1.2"",
      ""1.2.1"",
      ""1.2.2"",
      ""1.3.1""
    ],
    ""properties"": {
      ""1.1.1"": {
        ""description"": ""These are the input parameters for the Bicep module: [`main.bicep`](./main.bicep)\n\nThis is the orchestration module that is used and called by a consumer of the module to deploy a Landing Zone Subscription and its associated resources, based on the parameter input values that are provided to it at deployment time.\n\n> For more information and examples please see the [wiki](https://github.com/Azure/bicep-lz-vending/wiki)""
      },
      ""1.1.2"": {
        ""description"": ""These are the input parameters for the Bicep module: [`main.bicep`](./main.bicep)\n\nThis is the orchestration module that is used and called by a consumer of the module to deploy a Landing Zone Subscription and its associated resources, based on the parameter input values that are provided to it at deployment time.\n\n> For more information and examples please see the [wiki](https://github.com/Azure/bicep-lz-vending/wiki)""
      },
      ""1.2.1"": {
        ""description"": ""These are the input parameters for the Bicep module: [`main.bicep`](./main.bicep)\n\nThis is the orchestration module that is used and called by a consumer of the module to deploy a Landing Zone Subscription and its associated resources, based on the parameter input values that are provided to it at deployment time.\n\n> For more information and examples please see the [wiki](https://github.com/Azure/bicep-lz-vending/wiki)""
      },
      ""1.2.2"": {
        ""description"": ""These are the input parameters for the Bicep module: [`main.bicep`](./main.bicep)\n\nThis is the orchestration module that is used and called by a consumer of the module to deploy a Landing Zone Subscription and its associated resources, based on the parameter input values that are provided to it at deployment time.\n\n> For more information and examples please see the [wiki](https://github.com/Azure/bicep-lz-vending/wiki)""
      },
      ""1.3.1"": {
        ""description"": ""These are the input parameters for the Bicep module: [`main.bicep`](./main.bicep)\n\nThis is the orchestration module that is used and called by a consumer of the module to deploy a Landing Zone Subscription and its associated resources, based on the parameter input values that are provided to it at deployment time.\n\n> For more information and examples please see the [wiki](https://github.com/Azure/bicep-lz-vending/wiki)""
      }
    }
  },
  {
    ""moduleName"": ""network/dns-zone"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""network/nat-gateway"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""network/traffic-manager"",
    ""tags"": [
      ""1.0.1"",
      ""2.0.1"",
      ""2.1.1"",
      ""2.2.1"",
      ""2.3.1"",
      ""2.3.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""network/virtual-network"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2"",
      ""1.0.3"",
      ""1.1.1"",
      ""1.1.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""observability/grafana"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""samples/array-loop"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2""
    ],
    ""properties"": {
      ""1.0.2"": {
        ""description"": ""A sample Bicep registry module demonstrating array iterations.""
      }
    }
  },
  {
    ""moduleName"": ""samples/hello-world"",
    ""tags"": [
      ""1.0.1"",
      ""1.0.2"",
      ""1.0.3""
    ],
    ""properties"": {
      ""1.0.3"": {
        ""description"": ""A \""שָׁלוֹם עוֹלָם\"" sample Bicep registry module""
      }
    }
  },
  {
    ""moduleName"": ""security/keyvault"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""storage/cosmos-db"",
    ""tags"": [
      ""1.0.1"",
      ""2.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""storage/log-analytics-workspace"",
    ""tags"": [
      ""1.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""storage/redis-cache"",
    ""tags"": [
      ""0.0.1""
    ],
    ""properties"": {}
  },
  {
    ""moduleName"": ""storage/storage-account"",
    ""tags"": [
      ""0.0.1"",
      ""1.0.1"",
      ""2.0.1"",
      ""2.0.2""
    ],
    ""properties"": {}
  }
]";

        [TestMethod]
        public void GetExponentialDelay_ZeroCount_ShouldGiveInitialDelay()
        {
            TimeSpan initial = TimeSpan.FromDays(2.5);
            TimeSpan max = TimeSpan.FromDays(10);
            PublicRegistryModuleMetadataProvider provider = new();
            var delay = provider.GetExponentialDelay(initial, 0, max);

            delay.Should().Be(initial);
        }

        [TestMethod]
        public void GetExponentialDelay_1Count_ShouldGiveDoubleInitialDelay()
        {
            TimeSpan initial = TimeSpan.FromDays(2.5);
            TimeSpan max = TimeSpan.FromDays(10);
            PublicRegistryModuleMetadataProvider provider = new();
            var delay = provider.GetExponentialDelay(initial, 1, max);

            delay.Should().Be(initial*2);
        }

        [TestMethod]
        public void GetExponentialDelay_2Count_ShouldGiveQuadrupleInitialDelay()
        {
            TimeSpan initial = TimeSpan.FromDays(2.5);
            TimeSpan max = TimeSpan.FromDays(10);
            PublicRegistryModuleMetadataProvider provider = new();
            var delay = provider.GetExponentialDelay(initial, 2, max);

            delay.Should().Be(initial * 4);
        }

        [TestMethod]
        public void GetExponentialDelay_AboveMaxCount_ShouldGiveMaxDelay()
        {
            TimeSpan initial = TimeSpan.FromSeconds(1);
            TimeSpan max = TimeSpan.FromDays(365);
            PublicRegistryModuleMetadataProvider provider = new();

            TimeSpan exponentiallyGrowingDelay = initial;
            int count = 0;
            while (exponentiallyGrowingDelay < max * 1000)
            {
                var delay = provider.GetExponentialDelay(initial, count, max);

                if (exponentiallyGrowingDelay < max)
                {
                    delay.Should().Be(exponentiallyGrowingDelay);
                }
                else
                {
                    delay.Should().Be(max);
                }

                delay.Should().BeLessThanOrEqualTo(max);

                ++count;
                exponentiallyGrowingDelay *= 2;
            }
        }

        [TestMethod]
        public async Task asdfg()
        {
            PublicRegistryModuleMetadataProvider provider = new(ModuleIndex);
            var names = await provider.GetModuleNames();
            names.Should().HaveCount(1);
        }
    }
}
