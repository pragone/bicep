// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Configuration;
using Bicep.Core.Diagnostics;
using Bicep.Core.FileSystem;
using Bicep.Core.Modules;
using Bicep.Core.Registry;
using Bicep.Core.Syntax;
using Bicep.Core.UnitTests.Assertions;
using Bicep.Core.UnitTests.Mock;
using Bicep.Core.Workspaces;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using static Bicep.Core.Diagnostics.DiagnosticBuilder;

namespace Bicep.Core.UnitTests.Registry
{
    [TestClass]
    public class SourceBundleTests
    {
        [TestMethod]
        public void asdfg()
        {
            const string projectFolder = "/my project/my sources";
            //const string bundleFolder = "/my module cache/my sources";
            var fs = new MockFileSystem();
            fs.AddDirectory(projectFolder);
            const string mainBicepContents = @"targetScope = 'subscription'
metadata description = 'fake bicep file'";
            fs.AddFile(Path.Combine(projectFolder, "main.bicep"), mainBicepContents);

            var bicepMain = SourceFileFactory.CreateBicepFile(new Uri("file:///main.bicep"), mainBicepContents);
            using var stream = SourceBundle.PackSources(bicepMain.FileUri, bicepMain);

            using var test = File.OpenWrite("/Users/stephenweatherford/test.zip"); //asdfg
            stream.CopyTo(test);
            test.Close();

            stream.Seek(0, SeekOrigin.Begin);


            //SourceBundle.UnpackSources()
            SourceBundle sourceBundle = new SourceBundle(stream);
            //SourceBundle sourceBundle = new SourceBundle(fs, bundleFolder);

            //
        }
    }
}
