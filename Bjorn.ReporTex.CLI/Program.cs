using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bjorn.ReporTex.Stencil;

namespace Bjorn.ReporTex.CLI {
    class Program {
        static void Main(string[] args) {
            ITemplateEngine stencil = new Stencil.Stencil();
            IConverter converter = new Converter();

            string consoleOutput = String.Empty;

            IFile compiled = stencil.Compile("Sample_Data", null);
            //IFile compiled = stencil.Compile("Sample_Data", new { Author = "Chris", Title = "A Sample Report" });
            IBuiltFile built = converter.Convert(compiled, FileFormat.Pdf, out consoleOutput);
            //converter.Cleanup(built, true);

            Console.WriteLine(String.Format("\n{0}\n", consoleOutput));

            Console.WriteLine(String.Format("\nBuilt to: {0}", built.Path));
            Console.WriteLine("\nPress any key to exit...");
            Console.Read();
        }
    }
}
