// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Containers.ContainerRegistry;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Bicep.Core.Configuration;
using Bicep.Core.Registry.Auth;
using Bicep.Core.Tracing;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using static Bicep.Core.Emit.ResourceDependencyVisitor;

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

        private List<string> scopes = new(new string[] {
            //"repository:storage:pull",
            "https://sawbicep.azurecr.io/.default"
            });

        public async Task<HttpClient> CreateAuthenticatedHttpClientAsync(RootConfiguration configuration)
        {
            //var options = new ContainerRegistryClientOptions(); //asdfg not used
            //options.Diagnostics.ApplySharedContainerRegistrySettings();


            //var audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience); //asdfg not used


            //const environment = subscription.environment;
            //const resourceManagerEndpointUrl = environment.resourceManagerEndpointUrl;
            //const audience = environment.activeDirectoryResourceId;


            //var audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            //asdfg
            //options.Diagnostics.ApplySharedContainerRegistrySettings();
            //options.Audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

            //asdfg
            //TokenCredential credential = this.credentialFactory.CreateChain(configuration.Cloud.CredentialPrecedence, configuration.Cloud.ActiveDirectoryAuthorityUri);


            //string acrUrl = "https://sawbicep.azurecr.io";
            //string repositoryScope = /*"registry:catalog:*";*/ "repository:storage:pull";
            //string service = "sawbicep.azurecr.io";


            //string requestUrl = $"{acrUrl}/oauth2/token?scope={Uri.EscapeDataString(repositoryScope)}&service={Uri.EscapeDataString(service)}";





            //using var cts = new CancellationTokenSource(); //asdfg  timeout?
            //var accessToken = await credential.GetTokenAsync(new TokenRequestContext(scopes.ToArray()), cts.Token);


            //new TokenRequestContext(new[] {
            //    repositoryScope,
            //"sawbicep.azurecr.io/.default"}), cts.Token);//asdfg.ConfigureAwait(false);

            //AccessToken token = await credential.GetTokenAsync(new(), cts.Token);

            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            //return client;




            //var options = new ArmClientOptions();
            //options.Diagnostics.ApplySharedResourceManagerSettings();
            //options.Environment = new ArmEnvironment(new Uri(request.resourceManagerEndpointUrl), request.audience);

            //var credential = new CredentialFromTokenAndTimeStamp(request.token, request.expiresOnTimestamp);//asdfg?
            //var armClient = armClientProvider.createArmClient(credential, default, options);

            // HttpPipeline pipeline = new HttpPipeline(
            //new TokenCredentialAuthenticationPolicy(credential, "https://management.azure.com/.default"),
            //new HttpLoggingPolicy(),
            //new RetryPolicy(),
            //new BearerTokenAuthenticationPolicy(credential, "https://management.azure.com/.default"));


            //var options = new ArmClientOptions();
            //options.Diagnostics.ApplySharedResourceManagerSettings();
            //options.Environment = new ArmEnvironment(configuration.Cloud.ResourceManagerEndpointUri, configuration.Cloud.AuthenticationScope);


            //var credential = this.credentialFactory.CreateChain(configuration.Cloud.CredentialPrecedence, configuration.Cloud.ActiveDirectoryAuthorityUri);

            //options.
            //return new ArmClient(credential, null/*subscriptionId*/, options);



            //var options = new ArmClientOptions();
            //options.Diagnostics.ApplySharedResourceManagerSettings();
            //options.Environment = new ArmEnvironment(configuration.Cloud.ResourceManagerEndpointUri, configuration.Cloud.AuthenticationScope);

            var credential = this.credentialFactory.CreateChain(configuration.Cloud.CredentialPrecedence, configuration.Cloud.ActiveDirectoryAuthorityUri);
            //var armClient = new ArmClient(credential, subscriptionId, options);

            using var cts = new CancellationTokenSource(); //asdfg  timeout?
            var accessToken = await credential.GetTokenAsync(new TokenRequestContext(scopes.ToArray()), cts.Token);


            //new TokenRequestContext(new tokecon scopes, cts.Token);//asdfg.ConfigureAwait(false);

            AccessToken token = await credential.GetTokenAsync(new(), cts.Token);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            return client;


        }

        //public ArmClient CreateAnonymousHttpClientAsync(RootConfiguration configuration)
        //{
        //    throw new NotImplementedException(); //asdfg
        //    //var options = new ContainerRegistryClientOptions();
        //    //options.Diagnostics.ApplySharedContainerRegistrySettings();
        //    //options.Audience = new ContainerRegistryAudience(configuration.Cloud.ResourceManagerAudience);

        //    //return new(registryUri, repository, options);
        //}
    }
}
