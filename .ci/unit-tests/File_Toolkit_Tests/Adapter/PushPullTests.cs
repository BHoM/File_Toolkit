/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using NUnit.Framework;
using BH.Adapter.File;
using BH.oM.Structure.Elements;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;

namespace BH.Tests.Adapter
{
    public class PushPullTests
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

            var fileContent = fa.Pull(new BH.oM.Adapters.File.FileContentRequest() { File = randomTestFilePath });

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

            var fileContent = fa.Pull(new BH.oM.Adapters.File.FileContentRequest() { File = randomTestFilePath });

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

