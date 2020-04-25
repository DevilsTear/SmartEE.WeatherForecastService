using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Statik tanımlar
/// </summary>
public static class StaticDeclarations
{
    /// <summary>
    /// Klasör oluşturma kilit objesi
    /// </summary>
    static object createDirLock = new object();
    /// <summary>
    /// İstatistik dosyalarının tutulduğu varsayılan klasör yolu
    /// </summary>
    static string _StatsPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\stats";
    /// <summary>
    /// İstatistik dosyalarının tutulduğu klasör yolu
    /// </summary>
    public static string StatsPath
    {
        get
        {
            lock (createDirLock)
            {
                if (!Directory.Exists(_StatsPath))
                    Directory.CreateDirectory(_StatsPath);
            }

            return _StatsPath;
        }
    }

    /// <summary>
    /// Log dosyalarının tutulduğu varsayılan klasör yolu
    /// </summary>
    static string _LogPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\logs";
    /// <summary>
    /// Log dosyalarının tutulduğu klasör yolu
    /// </summary>
    public static string LogPath
    {
        get
        {
            lock (createDirLock)
            {
                if (!Directory.Exists(_LogPath))
                    Directory.CreateDirectory(_LogPath); 
            }

            return _LogPath;
        }
    }
}
