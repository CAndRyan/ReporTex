using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using RazorEngine.Text;
using System.Security.Policy;
using System.Security;
using System.Web.Razor;

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
        //private IRazorEngineService IsolatedRazorEngine;

        public Stencil () {
            Config = new TemplateServiceConfiguration();
            Config.Language = Language.CSharp;
            Config.EncodedStringFactory = new RawStringFactory();

            //IsolatedRazorEngine = IsolatedRazorEngineService.Create(new LanguageEncodingConfigCreator());
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

        //public static AppDomain SandboxCreator() {
        //    Evidence ev = new Evidence();
        //    ev.AddHostEvidence(new Zone(SecurityZone.Internet));
        //    PermissionSet permSet = SecurityManager.GetStandardSandbox(ev);
        //    // We have to load ourself with full trust
        //    StrongName razorEngineAssembly = typeof(RazorEngineService).Assembly.Evidence.GetHostEvidence<StrongName>();
        //    // We have to load Razor with full trust (so all methods are SecurityCritical)
        //    // This is because we apply AllowPartiallyTrustedCallers to RazorEngine, because
        //    // We need the untrusted (transparent) code to be able to inherit TemplateBase.
        //    // Because in the normal environment/appdomain we run as full trust and the Razor assembly has no security attributes
        //    // it will be completely SecurityCritical. 
        //    // This means we have to mark a lot of our members SecurityCritical (which is fine).
        //    // However in the sandbox domain we have partial trust and because razor has no Security attributes that means the
        //    // code will be transparent (this is where we get a lot of exceptions, because we now have different security attributes)
        //    // To work around this we give Razor full trust in the sandbox as well.
        //    StrongName razorAssembly = typeof(RazorTemplateEngine).Assembly.Evidence.GetHostEvidence<StrongName>();
        //    AppDomainSetup adSetup = new AppDomainSetup();
        //    adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        //    AppDomain newDomain = AppDomain.CreateDomain("Sandbox", null, adSetup, permSet, razorEngineAssembly, razorAssembly);
        //    return newDomain;
        //}
    }
    /// <summary>
    /// A helper interface to get a custom configuration into a new AppDomain.
    /// Classes inheriting this interface should be Serializable 
    /// (and not inherit from MarshalByRefObject).
    /// </summary>
    //public interface IConfigCreator {
    //    /// <summary>
    //    /// Create a new configuration instance.
    //    /// This method should be executed in the new AppDomain.
    //    /// </summary>
    //    /// <returns></returns>
    //    ITemplateServiceConfiguration CreateConfiguration();
    //}

    /// <summary>
    /// A simple <see cref="IConfigCreator"/> implementation to configure the <see cref="Language"/> and the <see cref="Encoding"/>.
    /// </summary>
    //[Serializable]
    //public class LanguageEncodingConfigCreator : IsolatedRazorEngineService.IConfigCreator {
    //    private Language language;
    //    private RazorEngine.Encoding encoding;

    //    /// <summary>
    //    /// Initializes a new <see cref="LanguageEncodingConfigCreator"/> instance
    //    /// </summary>
    //    /// <param name="language"></param>
    //    /// <param name="encoding"></param>
    //    public LanguageEncodingConfigCreator(Language language = Language.CSharp, RazorEngine.Encoding encoding = RazorEngine.Encoding.Raw) {
    //        this.language = language;
    //        this.encoding = encoding;
    //    }

    //    /// <summary>
    //    /// Create the configuration.
    //    /// </summary>
    //    /// <returns></returns>
    //    public ITemplateServiceConfiguration CreateConfiguration() {
    //        IEncodedStringFactory factory = null;
    //        switch (encoding) {
    //            case RazorEngine.Encoding.Html:
    //                factory = new HtmlEncodedStringFactory();
    //                break;
    //            case RazorEngine.Encoding.Raw:
    //                factory = new RawStringFactory();
    //                break;
    //            default:
    //                throw new NotImplementedException();
    //        }

    //        return new TemplateServiceConfiguration() {
    //            Language = language,
    //            EncodedStringFactory = factory
    //        };
    //    }
    //}
}
