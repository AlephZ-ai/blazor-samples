using Aspire.Hosting.Publishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting
{
    public static class AzureResourceExtensions
    {
        /// <summary>
        /// Adds an Azure OpenAI resource to the application model.
        /// </summary>
        /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
        /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
        /// <returns>A reference to the <see cref="IResourceBuilder{AzureOpenAIResource}"/>.</returns>
        public static IResourceBuilder<AzureOpenAIResource> AddAzureOpenAI(this IDistributedApplicationBuilder builder, string name)
        {
            var resource = new AzureOpenAIResource(name);
            return builder.AddResource(resource)
                .WithManifestPublishingCallback(WriteAzureOpenAIToManifest);
        }

        private static void WriteAzureOpenAIToManifest(ManifestPublishingContext context)
        {
            context.Writer.WriteString("type", "azure.openai.account.v0");
        }

        /// <summary>
        /// Adds an Azure OpenAI Deployment resource to the application model. This resource requires an <see cref="AzureOpenAIResource"/> to be added to the application model.
        /// </summary>
        /// <param name="serverBuilder">The Azure SQL Server resource builder.</param>
        /// <param name="name">The name of the deployment.</param>
        /// <returns>A reference to the <see cref="IResourceBuilder{AzureSqlDatabaseResource}"/>.</returns>
        public static IResourceBuilder<AzureOpenAIDeploymentResource> AddDeployment(this IResourceBuilder<AzureOpenAIResource> serverBuilder, string name)
        {
            var resource = new AzureOpenAIDeploymentResource(name, serverBuilder.Resource);
            return serverBuilder.ApplicationBuilder.AddResource(resource)
                                .WithManifestPublishingCallback(context => WriteAzureOpenAIDeploymentToManifest(context, resource));
        }

        private static void WriteAzureOpenAIDeploymentToManifest(ManifestPublishingContext context, AzureOpenAIDeploymentResource resource)
        {
            // Example:
            // "type": "azure.openai.deployment.v0",
            // "parent": "azureOpenAi",

            context.Writer.WriteString("type", "azure.openai.deployment.v0");
            context.Writer.WriteString("parent", resource.Parent.Name);
        }
    }
}
