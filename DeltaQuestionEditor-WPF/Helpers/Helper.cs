using MoreLinq;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeltaQuestionEditor_WPF.Helpers
{
    public static class Helper
    {
        public static string AppDataPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeltaQuestionEditor");
        }

        public static string AppDataPath(string path)
        {
            return Path.Combine(AppDataPath(), path);
        }

        public static void EnsurePathExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void ClearDirectory(string path)
        {
            foreach (string folder in Directory.EnumerateDirectories(path))
                Directory.Delete(folder, true);
            foreach (string file in Directory.EnumerateFiles(path))
                File.Delete(file);
        }

        public static void HideBoundingBox(object root)
        {
            Control control = root as Control;
            if (control != null)
                control.FocusVisualStyle = null;

            if (root is DependencyObject)
            {
                foreach (object child in LogicalTreeHelper.GetChildren((DependencyObject)root))
                {
                    HideBoundingBox(child);
                }
            }
        }

        [DllImport("shlwapi.dll", EntryPoint = "PathRelativePathTo")]
        static extern bool PathRelativePathTo(StringBuilder lpszDst, string from, UInt32 attrFrom, string to, UInt32 attrTo);

        public static string GetRelativePath(string from, string to)
        {
            StringBuilder builder = new StringBuilder(1024);
            bool result = PathRelativePathTo(builder, from, 0, to, 0);
            return builder.ToString();
        }

        public static bool Contains(this string str, IEnumerable<string> values)
        {
            return values.Any(str.Contains);
        }

        public static bool IsFileLocked(string file)
        {
            try
            {
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist
                return true;
            }

            //file is not locked
            return false;
        }

        public static void CreateEntryFromAny(this ZipArchive archive, string sourceName, string entryName = "")
        {
            var fileName = Path.GetFileName(sourceName);
            if (File.GetAttributes(sourceName).HasFlag(FileAttributes.Directory))
            {
                archive.CreateEntryFromDirectory(sourceName, Path.Combine(entryName, fileName));
            }
            else
            {
                archive.CreateEntryFromFile(sourceName, Path.Combine(entryName, fileName), CompressionLevel.Optimal);
            }
        }

        public static void CreateEntryFromDirectory(this ZipArchive archive, string sourceDirName, string entryName = "")
        {
            string[] files = Directory.GetFiles(sourceDirName).Concat(Directory.GetDirectories(sourceDirName)).ToArray();
            foreach (var file in files)
            {
                archive.CreateEntryFromAny(file, entryName);
            }
        }

        public static string NewGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string ToJSLiteral(this string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("JScript"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    string tmp = writer.ToString();
                    return writer.ToString();
                }
            }
        }

        public static uint ToArgb(this Color color)
        {
            return BitConverter.ToUInt32(new byte[] { color.B, color.G, color.R, color.A }, 0);
        }

        public static void Show(this UIElement element)
        {
            element.Visibility = Visibility.Visible;
        }

        public static void Hide(this UIElement element, bool collapse = true)
        {
            element.Visibility = collapse ? Visibility.Collapsed : Visibility.Hidden;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Blocks while condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="frequency">The frequency at which the condition will be check, in milliseconds.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        public static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (condition())
                    await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
                throw new TimeoutException();
        }

        /// <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition())
                    await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }

        public static string SHA1Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static Exception GetInnermostException(this Exception ex)
        {
            if (ex == null)
                return ex;
            else if (ex.InnerException == null)
                return ex;
            else
                return GetInnermostException(ex.InnerException);
        }

        public static string ExceptionToString(this Exception ex)
        {
            System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            string ret = string.Format("Exception caught in {0} v{1}\r\nHappened in {2}\r\n{3}", assemblyName.Name, assemblyName.Version, ex.Source, ex.ToString());
            if (ex.InnerException != null)
            {
                ret += "\r\n\r\nInner exception:\r\n" + ExceptionToString(ex.InnerException);
            }
            return ret;
        }
    }
}
