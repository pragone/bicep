// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bicep.Core.Resources;
using Bicep.Core.TypeSystem;
using Bicep.Core.TypeSystem.Az;

namespace Bicep.Wasm;

public class EmptyAzResourceTypeLoader : IAzResourceTypeLoader
{
    public ResourceTypeComponents LoadType(ResourceTypeReference reference)
        => throw new NotImplementedException();

    public IEnumerable<ResourceTypeReference> GetAvailableTypes()
        => ImmutableArray<ResourceTypeReference>.Empty;
}
