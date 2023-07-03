using NUnit.Framework;
using BH.Adapter.File;
using BH.oM.Structure.Elements;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;

namespace BH.Tests.Engine.Compute
{
    public class Compute
    {
        string randomTestFilePath = "";

        [SetUp]
        public void SetupRandomTestFilePath()
        {
            randomTestFilePath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() + ".json");
        }

        [TearDown]
        public void DeleteRandomTestFile()
        {
            if (File.Exists(randomTestFilePath))
                File.Delete(randomTestFilePath);
        }

        [Test]
        public void Bar()
        {
            Bar bar = (Bar)BH.Engine.Base.Create.RandomObject(typeof(Bar));

            FileAdapter fa = new FileAdapter(randomTestFilePath);
            var objectsToPush = new List<object>() { bar };
            fa.Push(objectsToPush, "", BH.oM.Adapter.PushType.DeleteThenCreate);

            if (!File.Exists(randomTestFilePath))
                Assert.Fail("File was not created.");

            var fileContent = BH.Engine.Adapters.File.Compute.ReadFromJsonFile(randomTestFilePath, true);

            fileContent.Should().BeEquivalentTo(objectsToPush);
        }

        [Test]
        public void Bar_FormattedJson()
        {
            Bar bar = (Bar)BH.Engine.Base.Create.RandomObject(typeof(Bar));

            FileAdapter fa = new FileAdapter(randomTestFilePath);
            var objectsToPush = new List<object>() { bar };
            fa.Push(objectsToPush, "", BH.oM.Adapter.PushType.DeleteThenCreate);

            if (!File.Exists(randomTestFilePath))
                Assert.Fail("File was not created.");

            // Format the json.
            OverwriteWithFormattedJson(randomTestFilePath);

            var fileContent = BH.Engine.Adapters.File.Compute.ReadFromJsonFile(randomTestFilePath, true);

            fileContent.Should().BeEquivalentTo(objectsToPush);
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private static void OverwriteWithFormattedJson(string filepath)
        {
            string json = File.ReadAllText(filepath);
            string formattedJson = FormatJson(json);
            File.WriteAllText(filepath, formattedJson);
        }
    }
}