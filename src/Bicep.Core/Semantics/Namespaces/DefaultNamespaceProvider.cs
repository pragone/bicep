// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Immutable;
using Bicep.Core.Extensions;
using Bicep.Core.Features;
using Bicep.Core.TypeSystem;
using Bicep.Core.TypeSystem.Az;
using Bicep.Core.TypeSystem.K8s;

namespace Bicep.Core.Semantics.Namespaces;

public class DefaultNamespaceProvider : INamespaceProvider
{
    private delegate NamespaceType GetNamespaceDelegate(string aliasName, ResourceScope resourceScope, IFeatureProvider features);
    private readonly ImmutableDictionary<string, GetNamespaceDelegate> providerLookup;

    public DefaultNamespaceProvider(AzResourceTypeProvider azResourceTypeProvider, K8sResourceTypeProvider k8sResourceTypeProvider)
    {
        this.providerLookup = new Dictionary<string, GetNamespaceDelegate>
        {
            [SystemNamespaceType.BuiltInName] = (alias, scope, features) => SystemNamespaceType.Create(alias, features),
            [AzNamespaceType.BuiltInName] = (alias, scope, features) => AzNamespaceType.Create(alias, scope, azResourceTypeProvider),
            [K8sNamespaceType.BuiltInName] = (alias, scope, features) => K8sNamespaceType.Create(alias, k8sResourceTypeProvider),
        }.ToImmutableDictionary();

        this.AvailableNamespaces = new [] {
            SystemNamespaceType.Settings,
            AzNamespaceType.Settings,
            K8sNamespaceType.Settings
        }.ToImmutableArray();
    }

    public NamespaceType? TryGetNamespace(string providerName, string aliasName, ResourceScope resourceScope, IFeatureProvider features)
        => providerLookup.TryGetValue(providerName)?.Invoke(aliasName, resourceScope, features);

    public ImmutableArray<NamespaceSettings> AvailableNamespaces { get; }
}
