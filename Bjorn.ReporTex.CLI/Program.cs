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

            string consoleOutput1 = String.Empty;
			string consoleOutput2 = String.Empty;

			IFile compiled1 = stencil.Compile("Sample_Data", null);
            IBuiltFile built1 = converter.Convert(compiled1, FileFormat.Pdf, out consoleOutput1);
			Console.WriteLine(String.Format("\n{0}\n", consoleOutput1));
			Console.WriteLine(String.Format("\nBuilt to: {0}", built1.Path));

			IFile compiled2 = stencil.Compile("Sample_Report", new { Author = "Chris", Title = "A Sample Report" });
			IBuiltFile built2 = converter.Convert(compiled2, FileFormat.Pdf, out consoleOutput2);
			converter.Cleanup(built2, true);
			Console.WriteLine(String.Format("\n{0}\n", consoleOutput2));
			Console.WriteLine(String.Format("\nBuilt to: {0}", built2.Path));

			Console.WriteLine("\nPress any key to exit...");
            Console.Read();
        }
    }
}
