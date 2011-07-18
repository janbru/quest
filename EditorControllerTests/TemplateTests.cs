﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void TestTemplates()
        {
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Substring(6).Replace("/", @"\");
            string templateFolder = System.IO.Path.Combine(folder, @"..\..\..\WorldModel\WorldModel\Core");
            Dictionary<string, string> templates = EditorController.GetAvailableTemplates(templateFolder);

            foreach (string template in templates.Values)
            {
                string tempFile = System.IO.Path.GetTempFileName();

                EditorController.CreateNewGameFile(tempFile, template, "Test");
                EditorController controller = new EditorController();
                string errorsRaised = string.Empty;
                
                controller.ShowMessage += (string message) =>
                {
                    errorsRaised += message;
                };

                bool result = controller.Initialise(tempFile, templateFolder);

                Assert.IsTrue(result, string.Format("Initialisation failed for template '{0}': {1}", System.IO.Path.GetFileName(template), errorsRaised));
                Assert.AreEqual(0, errorsRaised.Length, string.Format("Error loading game with template '{0}': {1}", System.IO.Path.GetFileName(template), errorsRaised));

                System.IO.File.Delete(tempFile);
            }
        }
    }
}