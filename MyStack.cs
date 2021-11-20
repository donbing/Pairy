using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

class StaticWebsiteStack : Stack
{
    [Output]
    public Output<string> StaticEndpoint { get; set; }

    [Output]
    public Output<string> PrimaryStorageKey { get; set; }

    public StaticWebsiteStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("resourceGroup");
        var storageAccount = CreateStorageAccount(resourceGroup);

        CreateStaticSiteStorage(resourceGroup, storageAccount);

        // Create web endpoint for index.html 
        this.StaticEndpoint = storageAccount.PrimaryEndpoints
            .Apply(primaryEndpoints => primaryEndpoints.Web);

        // Export the primary key of the Storage Account
        this.PrimaryStorageKey = Output
            .Tuple(resourceGroup.Name, storageAccount.Name)
            .Apply(names => Output.CreateSecret(GetStorageAccountPrimaryKey(names.Item1, names.Item2)));
    }

    private static void CreateStaticSiteStorage(ResourceGroup resourceGroup, StorageAccount storageAccount)
    {
        // Create static site storage
        var staticWebSite = new StorageAccountStaticWebsite(
            "StaticWebsite",
            new StorageAccountStaticWebsiteArgs
            {
                AccountName = storageAccount.Name,
                ResourceGroupName = resourceGroup.Name,
                IndexDocument = "index.html",
            });

        // Create blob store for index.html
        var index_html = new Blob(
            "index.html",
            new BlobArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = storageAccount.Name,
                ContainerName = staticWebSite.ContainerName,
                Source = new FileAsset("app/index.html"),
                ContentType = "text/html",
            });
    }

    private static StorageAccount CreateStorageAccount(ResourceGroup resourceGroup)
    {
        // Create an Azure resource (Storage Account)
        return new StorageAccount("sa", new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS,
            },
            Kind = Kind.StorageV2
        });
    }

    private static async Task<string> GetStorageAccountPrimaryKey(string resourceGroupName, string accountName)
    {
        var accountKeys = await ListStorageAccountKeys.InvokeAsync(new ListStorageAccountKeysArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = accountName
        });
        return accountKeys.Keys[0].Value;
    }
}
