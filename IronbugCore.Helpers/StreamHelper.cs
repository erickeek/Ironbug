namespace IronbugCore.Helpers;

public static class StreamHelper
{
    public static byte[] ToByteArray(this Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}