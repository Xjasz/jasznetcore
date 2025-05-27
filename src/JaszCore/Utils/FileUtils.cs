using JaszCore.Common;
using JaszCore.Core;
using JaszCore.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace JaszCore.Utils
{
    public static class FileUtils
    {
        private static LoggerService Log => ServiceLocator.Get<LoggerService>();

        public static DirectoryInfo EnsurePathExists(this FilePathUtils path)
        {
            Log.Debug($"EnsurePathExists {path}");

            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            if (path != null)
            {
                foreach (string name in path.Items)
                {
                    DirectoryInfo result = null;
                    try
                    {
                        result = current.GetDirectories(name)[0];
                    }
                    catch (Exception) { }

                    if (result == null)
                    {
                        result = current.CreateSubdirectory(name);
                    }
                    current = result;
                }
            }
            return current;
        }

        public static DirectoryInfo LookupFolder(this FilePathUtils path)
        {
            Log.Debug($"LookupFolder {path}");

            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            if (path != null)
            {
                foreach (string name in path.Items)
                {
                    DirectoryInfo result = null;
                    try
                    {
                        result = current.GetDirectories(name)[0];
                    }
                    catch (Exception) { }

                    if (result == null)
                    {
                        return null;
                    }
                    current = result;
                }
            }
            return current;
        }

        public static FileInfo CreateOrReplaceFile(this FilePathUtils path)
        {
            Log.Debug($"CreateOrReplaceFile {path}");

            string fileName = path.Name;
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo file = null;
            try { file = folder.GetFiles(fileName)[0]; } catch (Exception) { }

            if (file != null)
            {
                file.Delete();
            }
            file.Create();
            return file;
        }

        public static FileInfo CreateFile(this FilePathUtils path)
        {
            Log.Debug($"CreateFile {path}");

            string fileName = path.Name;
            DirectoryInfo folder = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo file = null;
            try { file = folder.GetFiles(fileName)[0]; } catch (Exception) { }
            if (file == null)
            {
                file.Create();
            }
            return file;
        }


        public static void WriteToPathAsync(this FilePathUtils path, byte[] data)
        {
            Log.Debug($"WriteToPath {path}");
            Log.Debug($"WriteData {data}");
            path.CreateOrReplaceFile();
        }

        public static void SaveToPath(this FilePathUtils path, object value)
        {
            Log.Debug($"SaveToPath {path}");
            Log.Debug($"WriteObject {value}");
            path.CreateOrReplaceFile();
        }

        public static async Task WriteToFile(this FileInfo file, byte[] value)
        {
            Log.Debug($"WriteToFile {file.FullName}");
            FileStream stream = file.OpenWrite();
            await stream.WriteAsync(value, 0, value.Length);
            stream.Flush();
        }

        public static async Task SaveToFile(this FileInfo file, object value)
        {
            Log.Debug($"SaveToFile {file.FullName}, {value}");
            FileStream stream = file.OpenWrite();
            await Task.Run(() =>
            {
                //stream.WriteAsync(value, 0, value.Length());
            });
            stream.Flush();

        }

        public static async Task SaveToFileWithCompression(this FileInfo file, object value)
        {
            if (file == null)
            {
                return;
            }

            Log.Debug($"SaveToFileWithCompression {file} {value}");
            FileStream stream = file.OpenWrite();
            await Task.Run(() =>
            {
                //stream.WriteAsync(value, 0, value.Length());
            });
            stream.Flush();
        }


        public static T LoadFromFileWithDecompression<T>(this FileInfo file)
        {

            if (file == null)
            {
                return default;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Log.Debug($"LoadFromFileWithDecompression {file.FullName}");

            try
            {
                FileStream stream = file.OpenWrite();
                return default;
                //return await Task.Run(() => { return (T)new CHessianInput(stream).ReadObject(typeof(T)); });
            }
            catch (Exception e)
            {
                Log.Error(e, $"LoadFromFileWithDecompression: {file}");
                return default;
            }
            finally
            {
                sw.Stop();
                Log.Debug($"File {file.Name} took {sw.Elapsed} ms to load");
            }
        }



        public static async Task<T> LoadFromFile<T>(this FileInfo file)
        {

            if (file == null)
            {
                return default;
            }

            Log.Debug($"LoadFromFile {file.FullName}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                FileStream stream = file.OpenWrite();
                return await Task.Run(() => { return default(T); });
                // CHessianInput io = new CHessianInput(stream);
                // T result = (T)io.ReadObject(typeof(T));
                // io.GetStatistics()?.Print();
                // return result;
                // });
            }
            catch (Exception e)
            {
                Log.Error(e, "Filed to load from file {0}", 0, file.Name);
                return default;
            }
            finally
            {
                sw.Stop();
                Log.Debug($"File {file.Name} took {sw.Elapsed} ms to load");
            }
        }

        public static async Task<T> LoadFromPath<T>(this FilePathUtils path)
        {
            Log.Debug($"LoadFromPath {path}");

            return await path.LoadFromPath<T>();
        }


        public static async Task<byte[]> ReadFromFile(this FileInfo file)
        {
            if (file == null)
            {
                return null;
            }

            Log.Debug($"ReadFromFile {file.FullName}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                FileStream stream = file.OpenRead();
                MemoryStream result = new MemoryStream(4048);
                await stream.CopyToAsync(result);
                return result.ToArray();
            }
            catch (Exception e)
            {
                Log.Error(e, "Filed to read from file {0}", 0, file.Name);
                return null;
            }
            finally
            {
                sw.Stop();
                Log.Debug($"File {file.Name} took {sw.Elapsed} ms to load");
            }
        }

        public static async Task<byte[]> ReadFromPath(this FilePathUtils path)
        {
            Log.Debug($"ReadFromPath {path}");

            return await path.LoadFromPath<byte[]>();
        }

        public static async Task<T> LoadFromPathWithDecompression<T>(this FilePathUtils path)
        {
            Log.Debug($"LoadFromPathWithDecompression {path}");

            return await path.LoadFromPath<T>();
        }

        public static FileInfo LookupFileByPath(this FilePathUtils path)
        {
            Log.Debug($"LookupFileByPath {path}");

            try
            {
                DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent;
                if (current != null)
                {
                    return current.GetFiles(path.Name)[0]; ;
                }
            }
            catch (Exception) { }
            return null;
        }

    }

    public class IFileByModifyDateComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo x, FileInfo y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}