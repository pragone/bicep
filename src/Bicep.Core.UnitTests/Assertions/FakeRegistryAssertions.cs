// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Registry.Oci;
using Bicep.Core.UnitTests.Registry;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace Bicep.Core.UnitTests.Assertions
{
    public static class FakeRegistryBlobClientExtensions
    {
        public static MockRegistryAssertions Should(this FakeRegistryClient client) => new MockRegistryAssertions(client);
    }

    public class MockRegistryAssertions : ReferenceTypeAssertions<FakeRegistryClient, MockRegistryAssertions>
    {
        public MockRegistryAssertions(FakeRegistryClient client)
            : base(client)
        {
        }

        protected override string Identifier => nameof(FakeRegistryClient);

        public AndConstraint<MockRegistryAssertions> HaveModule(string tag, Stream expectedModuleContent)
        {
            using (new AssertionScope())
            {
                this.Subject.ManifestTags.Should().ContainKey(tag, $"tag '{tag}' should exist");

                string manifestDigest = this.Subject.ManifestTags[tag];

                this.Subject.Manifests.Should().ContainKey(manifestDigest, $"tag '{tag}' resolves to digest '{manifestDigest}' that should exist");

                var manifestBytes = this.Subject.Manifests[manifestDigest];
                using var manifestStream = FakeRegistryClient.WriteStream(manifestBytes);
                var manifest = OciSerialization.Deserialize<OciManifest>(manifestStream);

                manifest.ArtifactType.Should().Be("application/vnd.ms.bicep.module.artifact", "artifact type should be correct");

                var config = manifest.Config;
                config.MediaType.Should().Be("application/vnd.ms.bicep.module.config.v1+json", "config media type should be correct");
                config.Size.Should().Be(0, "config should be empty");

                this.Subject.Blobs.Should().ContainKey(config.Digest, "config digest should exist");

                var configBytes = this.Subject.Blobs[config.Digest];
                configBytes.Should().BeEmpty("config should be empty");

                manifest.Layers.Should().HaveCount(1, "modules should have a single layer");
                var layer = manifest.Layers.Single();

                layer.MediaType.Should().Be("application/vnd.ms.bicep.module.layer.v1+json", "layer media type should be correct");
                this.Subject.Blobs.Should().ContainKey(layer.Digest);

                var layerBytes = this.Subject.Blobs[layer.Digest];
                ((long)layerBytes.Length).Should().Be(layer.Size);

                var actual = FakeRegistryClient.WriteStream(layerBytes).FromJsonStream<JToken>();
                var expected = expectedModuleContent.FromJsonStream<JToken>();

                actual.Should().DeepEqual(expected, "module content should match");
            }

            return new(this);
        }

        public AndConstraint<MockRegistryAssertions> OnlyHaveModule(string tag, Stream expectedModuleContent)
        {
            using (new AssertionScope())
            {
                this.Subject.Should().HaveModule(tag, expectedModuleContent);

                // we should only have an empty config blob and the module layer
                this.Subject.Blobs.Should().HaveCount(2);

                // there should be one manifest for one module
                this.Subject.Manifests.Should().HaveCount(1);

                // there should only be one tag
                this.Subject.ManifestTags.Should().HaveCount(1);
            }

            return new(this);
        }
    }
}