using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace V8Module.BasicApi
{
    class CFile
    {
        public string getContents(string filename)
        {
            return File.ReadAllText(filename);
        }

        public void putContents(string filename, string content)
        {
            File.WriteAllText(filename, content);
        }

        public void Create(string filePath)
        {
            File.Create(filePath);
        }

        public void Delete(string filePath)
        {
            File.Delete(filePath);
        }

        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

    }

    class CDirectory
    {
        public string[] getFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string[] getDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public void Create(string path)
        {
            Directory.CreateDirectory(path);
        }
        public void Delete(string path)
        {
            Directory.Delete(path);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
