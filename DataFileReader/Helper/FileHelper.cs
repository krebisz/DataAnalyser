namespace DataFileReader.Helper;

public static class FileHelper
{
    public static List<string> GetFileList(string directory)
    {
        var fileList = new List<string>();

        var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
            fileList.Add(file);

        return fileList;
    }

    public static string GetFileName(string file)
    {
        var fileName = string.Empty;

        char[] separator =
        {
                '/',
                '\\'
        };
        var fileParts = file.Split(separator);

        var filePartsLength = fileParts.Length;

        if (filePartsLength > 0)
            fileName = fileParts[filePartsLength - 1].Trim().ToLower();

        fileName = DataHelper.RemoveSpecialCharacters(fileName);

        return fileName;
    }

    public static string GetFileExtension(string file)
    {
        var fileExtension = string.Empty;

        var fileParts = file.Split('.');

        var filePartsLength = fileParts.Length;

        if (filePartsLength > 0)
            fileExtension = fileParts[filePartsLength - 1].Trim().ToLower();

        return fileExtension;
    }

    public static int DeleteEmptyFiles(List<string> fileList)
    {
        var filesDeleted = 0;

        foreach (var file in fileList)
            try
            {
                if (File.Exists(file))
                {
                    var sizeInBytes = new FileInfo(file).Length;

                    if (sizeInBytes == 0)
                    {
                        File.Delete(file);
                        filesDeleted++;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

        return filesDeleted;
    }
}