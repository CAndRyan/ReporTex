using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bjorn.ReporTex.Stencil {
    public enum FileFormat {
        Pdf
    }

    public interface IConverter {
        IBuiltFile Convert(IFile source, FileFormat format);
        IBuiltFile Convert(IFile source, FileFormat format, out string consoleOutput);
        IBuiltFile Convert(string sourceFile, FileFormat format);
        IBuiltFile Convert(string sourceFile, FileFormat format, out string consoleOutput);
        void Cleanup(IBuiltFile built, bool removeSource);
    }

    public class Converter : IConverter {
        public static string OUTPUT_DIRECTORY { get; } = SystemHelper.CombineWithAssemblyPath("Built");
        public static string PDFLATEX_EXE_PATH { get; } = "C:\\Program Files\\MiKTeX 2.9\\miktex\\bin\\x64\\pdflatex.exe";
        public static int TIMEOUT_MS { get; } = 30000; // 30 seconds

        public string OutputDirectory { get; private set; } = OUTPUT_DIRECTORY;
        public int TimeoutMs { get; private set; } = TIMEOUT_MS;

        public void SetOutputDirectory(string path) {
            OutputDirectory = path;
        }

        public void SetTimeoutMs(int timeout) {
            TimeoutMs = timeout;
        }

        public IBuiltFile Convert(IFile source, FileFormat format, out string consoleOutput) {
            switch (format) {
                case FileFormat.Pdf:
                    switch (source.Extension) {
                        case ".tex":
                            return TexToPdf(source, out consoleOutput);
                        default:
                            throw new NotSupportedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public IBuiltFile Convert(IFile source, FileFormat format) {
            string str = null;
            return Convert(source, format, out str);
        }

        public IBuiltFile Convert(string filePath, FileFormat format, out string consoleOutput) {
            IFile source = SystemHelper.GetFileInfo(filePath);
            return Convert(source, format, out consoleOutput);
        }

        public IBuiltFile Convert(string filePath, FileFormat format) {
            IFile source = SystemHelper.GetFileInfo(filePath);
            return Convert(source, format);
        }

        public IBuiltFile TexToPdf(IFile source, out string consoleOutput) {
            BuiltFile built = null;

            List<string> parameters = new List<string>();
            parameters.Add(String.Format("\"{0}\"", source.Path));
            parameters.Add(String.Format("-output-directory=\"{0}\"", OutputDirectory));
            parameters.Add("-interaction=nonstopmode");

            consoleOutput = SystemHelper.ExecuteShellCommand(PDFLATEX_EXE_PATH, parameters, TimeoutMs);
            IEnumerable<IFile> buildFiles = SystemHelper.GetFileInfo(OutputDirectory, source.Name);

            Regex regex = new Regex(@"Output written on ""([^""]*)""");
            Match match = regex.Match(consoleOutput);

            if (match.Success) {
                built = new BuiltFile(match.Groups[1].Value, source, buildFiles);
            }
            else {  //fixme - consider removing
                built = new BuiltFile(SystemHelper.GetFilePath(OutputDirectory, source.Name, "tex"), source);
            }

            return built;
        }

        public IBuiltFile TexToPdf(IFile source) {
            string str = null;
            return TexToPdf(source, out str);
        }

        public void Cleanup(IBuiltFile built, bool removeSource = false) {
            foreach (IFile file in built.BuildFiles) {
                file.Delete();
            }

            if (removeSource) {
                built.BuildSource.Delete();
            }
        }
    }

    public interface IBuiltFile : IFile {
        IEnumerable<IFile> BuildFiles { get; }
        IFile BuildSource { get; }
    }

    public class BuiltFile : FileDetails, IBuiltFile {
        private List<IFile> _BuildFiles = new List<IFile>();
        public IEnumerable<IFile> BuildFiles {
            get {
                return _BuildFiles;
            }
        }
        public IFile BuildSource { get; }

        public BuiltFile(string path, IFile source) : base(path) {
            BuildSource = source;
        }

        public BuiltFile(string path, IFile source, IEnumerable<IFile> buildFiles) : this(path, source) {
            foreach(IFile file in buildFiles) {
                AddBuildFile(file);
            }
        }

        public BuiltFile(string path, IFile source, IFile buildFile) : this(path, source) {
            AddBuildFile(buildFile);
        }

        public void AddBuildFile(IFile file) {
            if (this.Equals(file)) {
                return;
            }
            else if (BuildSource.Equals(file)) {
                return;
            }
            else if (BuildFiles.Where(f => f.Equals(file)).Count() == 0) {
                _BuildFiles.Add(file);
            }
        }

        public void AddBuildFile(string location, string extension) {
            AddBuildFile(SystemHelper.GetFileInfo(location, this.Name, extension));
        }
    }
}
