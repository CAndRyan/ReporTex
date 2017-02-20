using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using RazorEngine.Text;

namespace Bjorn.ReporTex.Stencil {
    public interface ITemplateEngine {
        IFile Compile(string templateName, object model);
    }

    public class Stencil : ITemplateEngine {
        public static string TEMPLATE_DIRECTORY { get; } = SystemHelper.CombineWithAssemblyPath("Templates");
        public static string OUTPUT_DIRECTORY { get; } = SystemHelper.CombineWithAssemblyPath("Built");

        public string TemplateDirectory { get; private set; } = TEMPLATE_DIRECTORY;
        public string OutputDirectory { get; private set; } = OUTPUT_DIRECTORY;
        private TemplateServiceConfiguration Config;

        public Stencil () {
            Config = new TemplateServiceConfiguration();
            Config.Language = Language.CSharp;
            Config.EncodedStringFactory = new RawStringFactory();
            
            Engine.Razor = RazorEngineService.Create(Config);
        }

        public void SetTemplateDirectory(string path) {
            TemplateDirectory = path;
        }

        public void SetOutputDirectory(string path) {
            OutputDirectory = path;
        }

        public IFile Compile(string templateName, object model) {
            IFile source = SystemHelper.GetFileInfo(TemplateDirectory, templateName, "cstex");

            string src = SystemHelper.ReadFile(source.Path);
            string tex = Engine.Razor.RunCompile(src, source.Name, null, model);
            string output = SystemHelper.GetFilePath(OutputDirectory, source.Name, "tex");
            return SystemHelper.WriteFile(output, tex);
        }
    }
}
