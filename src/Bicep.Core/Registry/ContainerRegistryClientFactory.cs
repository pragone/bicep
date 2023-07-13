// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Containers.ContainerRegistry;
using Azure.Core;
using Bicep.Core.Configuration;
using Bicep.Core.Registry.Auth;
using Bicep.Core.Tracing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bicep.Core.Registry
{
    public class ContainerRegistryClientFactory : IContainerRegistryClientFactory
    {
        private readonly ITokenCredentialFactory credentialFactory;

        public ContainerRegistryClientFactory(ITokenCredentialFactory credentialFactory)
        {
            this.credentialFactory = credentialFactory;
        }

        public ContainerRegistryContentClient CreateAuthenticatedBlobClient(RootConfiguration configuration, Uri registryUri, string repository)
        {
            var options = new ContainerRegistryClientOptions();
            options.Diagnostics.ApplySharedContainerRegistrySettings();
            options.Audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            //asdfg
            var credential = this.credentialFactory.CreateChain(configuration.Cloud.CredentialPrecedence, configuration.Cloud.ActiveDirectoryAuthorityUri);

            return new(registryUri, repository, credential, options);
        }

        public ContainerRegistryContentClient CreateAnonymousBlobClient(RootConfiguration configuration, Uri registryUri, string repository)
        {
            var options = new ContainerRegistryClientOptions();
            options.Diagnostics.ApplySharedContainerRegistrySettings();
            options.Audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            return new(registryUri, repository, options);
        }

        public async HttpClient CreateAuthenticatedHttpClientAsync(RootConfiguration configuration, Uri uri)
        {
            var options = new ContainerRegistryClientOptions();
            options.Diagnostics.ApplySharedContainerRegistrySettings();
            var audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);


            //const environment = subscription.environment;
            //const resourceManagerEndpointUrl = environment.resourceManagerEndpointUrl;
            //const audience = environment.activeDirectoryResourceId;


            //var audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            //asdfg
            //options.Diagnostics.ApplySharedContainerRegistrySettings();
            //options.Audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            //asdfg
            var credential = this.credentialFactory.CreateChain(configuration.Cloud.CredentialPrecedence, configuration.Cloud.ActiveDirectoryAuthorityUri);
            AccessToken token = await credential.GetTokenAsync(new TokenRequestContext());

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential.GetTokenAsync());
        }

        public HttpClient CreateAnonymousHttpClient(RootConfiguration configuration, Uri uri)
        {
            var options = new ContainerRegistryClientOptions();
            options.Diagnostics.ApplySharedContainerRegistrySettings();
            options.Audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            return new(registryUri, repository, options);
        }
    }
}
