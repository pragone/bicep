// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Bicep.Core.Workspaces;
using Bicep.LanguageServer.CompilationManager;
using Bicep.LanguageServer.Registry;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Bicep.Wasm;

public class EmptyModuleRestoreScheduler : IModuleRestoreScheduler
{
    public void RequestModuleRestore(ICompilationManager compilationManager, DocumentUri documentUri, IEnumerable<ModuleSourceResolutionInfo> references)
    {
    }

    public void Start()
    {
    }
}
