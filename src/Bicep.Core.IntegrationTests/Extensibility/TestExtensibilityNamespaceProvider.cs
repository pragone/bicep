// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Bicep.Core.TypeSystem;
using Bicep.Core.Semantics.Namespaces;
using Bicep.Core.Features;
using Bicep.Core.UnitTests;
using System.Collections.Immutable;

namespace Bicep.Core.IntegrationTests.Extensibility
{
    public class TestExtensibilityNamespaceProvider : INamespaceProvider
    {
        private readonly INamespaceProvider defaultNamespaceProvider;

        public TestExtensibilityNamespaceProvider()
        {
            defaultNamespaceProvider = BicepTestConstants.NamespaceProvider;
        }

        public ImmutableArray<NamespaceSettings> AvailableNamespaces => defaultNamespaceProvider.AvailableNamespaces.AddRange(new [] {
            StorageNamespaceType.Settings,
            AadNamespaceType.Settings,
        });

        public NamespaceType? TryGetNamespace(string providerName, string aliasName, ResourceScope resourceScope, IFeatureProvider featureProvider)
        {
            if (defaultNamespaceProvider.TryGetNamespace(providerName, aliasName, resourceScope, featureProvider) is { } namespaceType)
            {
                return namespaceType;
            }

            switch (providerName)
            {
                case StorageNamespaceType.BuiltInName:
                    return StorageNamespaceType.Create(aliasName);
                case AadNamespaceType.BuiltInName:
                    return AadNamespaceType.Create(aliasName);
            }

            return default;
        }
    }
}
