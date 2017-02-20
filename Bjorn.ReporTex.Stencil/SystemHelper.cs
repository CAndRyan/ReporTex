using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bjorn.ReporTex.Stencil {
    public class SystemHelper {
        public static string AssemblyDirectory {
            get {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        //public static string GetRelativePath(string fromPath, string toPath) {
        //    if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
        //    if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

        //    Uri fromUri = new Uri(fromPath);
        //    Uri toUri = new Uri(toPath);

        //    if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

        //    Uri relativeUri = fromUri.MakeRelativeUri(toUri);
        //    String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        //    if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
        //        relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        //    }

        //    relativePath = String.Format(".\\{0}", relativePath.Substring(relativePath.IndexOf('\\') + 1));

        //    return relativePath;
        //}

        //public static string GetRelativePathFromAssembly(string referencePath) {
        //    return GetRelativePath(AssemblyDirectory, referencePath);
        //}

        public static string GetFilePath(string directory, string fileName, string extension) {
            extension = extension.TrimStart();
            extension = extension.Replace(".", "");

            return Path.Combine(directory, String.Format("{0}.{1}", fileName, extension));
        }

        public static string GetPath(string path1, string path2) {
            return Path.Combine(path1, path2);
        }

        public static string CombineWithAssemblyPath(string path) {
            return Path.Combine(AssemblyDirectory, path);
        }

        public static string ReadFile(string path) {
            return File.ReadAllText(path);
        }

        public static IFile WriteFile(string path, string text) {
            File.WriteAllText(path, text);

            return GetFileInfo(path);
        }

        public static IFile GetFileInfo(string path) {
            return new FileDetails(path);
        }

        public static IFile GetFileInfo(string directory, string fileName, string extension) {
            return GetFileInfo(GetFilePath(directory, fileName, extension));
        }

        public static IEnumerable<IFile> GetFileInfo(string directory, string fileName) {
            List <IFile> files = new List<IFile>();
            string[] paths = Directory.GetFiles(directory, String.Format("{0}*", fileName));

            foreach (string path in paths) {
                files.Add(GetFileInfo(path));
            }

            return files;
        }

        // MSDN --> http://msdn.microsoft.com/en-us/library/system.diagnostics.process.standardoutput.aspx
        public static string ExecuteShellCommand(string executable, IEnumerable<string> parameters, int timeoutMs = Int32.MaxValue, string workingDirectory = null) {
            // Start the child process.
            Process proc = new Process();

            // Redirect the output stream of the child process.
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;

            // Configure the executable with parameters
            proc.StartInfo.FileName = executable;
            proc.StartInfo.WorkingDirectory = (workingDirectory == null) ? AssemblyDirectory : workingDirectory;
            proc.StartInfo.Arguments = String.Join(" ", parameters);

            // Start the process
            proc.Start();

            // Do not wait for the child process to exit before reading to the end of its redirected stream.
            // Read the output stream first and then wait.
            string output = proc.StandardOutput.ReadToEnd().Replace("\r\n", "");
            proc.WaitForExit(timeoutMs);

            return output;
        }

        public static void DeleteFile(string path) {
            File.Delete(path);
        }
    }

    public interface IFile {
        string Path { get; }
        string Extension { get; }
        string Name { get; }
        string Location { get; }
        bool Deleted { get; }

        void Delete();
        bool Equals(IFile file);
    }

    public class FileDetails : IFile {
        private FileInfo Source;
        public string Path {
            get {
                return Source.FullName;
            }
        }
        public string Extension {
            get {
                return Source.Extension;
            }
        }
        public string Name {
            get {
                return Source.Name.Replace(Extension, "");
            }
        }
        public string Location {
            get {
                return Source.DirectoryName;
            }
        }
        public bool Deleted { get; private set; } = false;

        public FileDetails(FileInfo source) {
            Source = source;
        }

        public FileDetails(string sourcePath) {
            Source = new FileInfo(sourcePath);
        }

        public void Delete() {
            SystemHelper.DeleteFile(Path);
            Deleted = true;
        }

        public bool Equals(IFile file) {
            return String.Equals(Path, file.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(Object obj) {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            IFile file = (IFile)obj;
            return Equals(file);
        }

        public override int GetHashCode() {
            return Path.GetHashCode();
        }
    }
}
