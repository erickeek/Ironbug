using System.IO;

namespace IronBug.Helpers
{
    public static class FileHelper
    {
        public static void SaveToFile(this byte[] bytes, string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                file.Write(bytes, 0, bytes.Length);
                file.Close();
            }
        }
    }
}
