using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PublishTools;

public static class MingwDeps
{
    public static string Bash(string command, bool print = true)
    {
        Console.WriteLine(command);
        var psi = new ProcessStartInfo("/usr/bin/env", "bash");
        psi.RedirectStandardInput = true;
        psi.RedirectStandardOutput = true;
        var process = Process.Start(psi);
        process.StandardInput.Write(command);
        process.StandardInput.Close();
        var task = process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();
        task.Wait();
        if (process.ExitCode != 0) throw new Exception($"Command Failed: {command}");
        return task.Result.Trim();
    }
    public static void CopyFile(string src, string dst)
    {
        if (Directory.Exists(dst))
            dst = Path.Combine(dst, Path.GetFileName(src));
        File.Copy(src, dst, true);
    }
    
    public static void CopyMingwDependencies(string prefix, string targetfolder)
    {
        var deps = new HashSet<string>();
        for (int i = 0; i < 2; i++) //Search twice
        {
            foreach (var f in Directory.GetFiles(targetfolder, "*.dll"))
            {
                var dependencies = GetDllsForFile(prefix, f);
                foreach (var d in dependencies)
                    if (!deps.Contains(d))
                        deps.Add(d);
            }
            foreach (var d in deps)
            {
                var f = FindFile(prefix, d);
                if (f != null)
                {
                    Console.WriteLine($"Copying dependency: {f}");
                    CopyFile(f, Path.Combine(targetfolder, d));
                }
            }
        }
    }
    
    public static string Quote(string s)
    {
        return $"\"{s.Replace("\"", "\\\"")}\"";
    }
    static string FindFile(string prefix, string file)
    {
        var f = Directory.GetFiles($"/usr/{prefix}", file, SearchOption.AllDirectories).FirstOrDefault();
        if(f == null && Directory.Exists($"/usr/lib/gcc/{prefix}")) {
            var posixDir = Directory.GetDirectories($"/usr/lib/gcc/{prefix}").FirstOrDefault(x => x.EndsWith("posix"));
            if(posixDir != null)
                f = Directory.GetFiles(posixDir, file, SearchOption.AllDirectories).FirstOrDefault();
        }
        return f;
    }
    static string[] GetDllsForFile(string prefix, string file)
    {
        return Bash(
                $"{prefix}-objdump -p {Quote(file)} | grep 'DLL Name:' | sed -e \"s/\t*DLL Name: //g\" | grep '^lib' | cat")
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}