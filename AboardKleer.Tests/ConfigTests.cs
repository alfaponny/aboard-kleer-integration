using Microsoft.Extensions.Configuration;
using AboardKleerIntegration.Config;

namespace AboardKleerIntegration.Tests;

[TestClass]
public class ConfigTests
{
        
    [TestMethod]
    public void KleerOptions_BindsAllProperties()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kleer:BaseUrl"] = "https://api.kleer.test",
                ["Kleer:ApiKey"] = "test-api-key",
                ["Kleer:CompanyId"] = "123"
            })
            .Build();

        var options = new KleerOptions();
        config.GetSection("Kleer").Bind(options);

        Assert.IsFalse(string.IsNullOrEmpty(options.BaseUrl), "Kleer BaseUrl should not be empty");
        Assert.IsFalse(string.IsNullOrEmpty(options.ApiKey), "Kleer ApiKey should not be empty");
        Assert.IsFalse(string.IsNullOrEmpty(options.CompanyId), "Kleer CompanyId should not be empty");
        Assert.AreEqual("https://api.kleer.test", options.BaseUrl);
        Assert.AreEqual("test-api-key", options.ApiKey);
        Assert.AreEqual("123", options.CompanyId);
    }

    [TestMethod]
    public void AboardOptions_BindsAllProperties()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Aboard:BaseUrl"] = "https://api.aboard.test",
                ["Aboard:ApiKey"] = "test-api-key"
            })
            .Build();

        var options = new AboardOptions();
        config.GetSection("Aboard").Bind(options);

        Assert.IsFalse(string.IsNullOrEmpty(options.BaseUrl), "Aboard BaseUrl should not be empty");
        Assert.IsFalse(string.IsNullOrEmpty(options.ApiKey), "Aboard ApiKey should not be empty");
        Assert.AreEqual("https://api.aboard.test", options.BaseUrl);
        Assert.AreEqual("test-api-key", options.ApiKey);
    }

    [TestMethod]
    public void SlackOptions_BindsAllProperties()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Slack:WebhookUrl"] = "https://hooks.slack.com/services/test-webhook"
            })
            .Build();

        var options = new SlackOptions();
        config.GetSection("Slack").Bind(options);

        Assert.IsFalse(string.IsNullOrEmpty(options.WebhookUrl), "Slack WebhookUrl should not be empty");
        Assert.AreEqual("https://hooks.slack.com/services/test-webhook", options.WebhookUrl);
    }

     [TestMethod]
     public void Options_DefaultToEmptyStrings_WhenSectionMissing()
     {
         var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
        
        var kleer= new KleerOptions();
        config.GetSection("Kleer").Bind(kleer);

        Assert.AreEqual(string.Empty, kleer.BaseUrl);
        Assert.AreEqual(string.Empty, kleer.ApiKey);
        Assert.AreEqual(string.Empty, kleer.CompanyId);
     }  
}