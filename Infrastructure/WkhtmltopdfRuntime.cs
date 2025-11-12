using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Infrastructure
{
    internal sealed class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath) => LoadUnmanagedDllFromPath(absolutePath);
        protected override Assembly? Load(AssemblyName assemblyName) => null;
    }

    public static class WkhtmltopdfRuntime
    {
        private static bool _loaded;
        public static void EnsureLoaded()
        {
            if (_loaded) return;
            var baseDir = AppContext.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, "libwkhtmltox", "libwkhtmltox.dll"),
                Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox", "libwkhtmltox.dll"),
                Path.Combine(baseDir, "runtimes", "win-x64", "native", "libwkhtmltox.dll"),
            };
            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    var context = new CustomAssemblyLoadContext();
                    context.LoadUnmanagedLibrary(path);
                    _loaded = true;
                    return;
                }
            }
        }
    }
}

