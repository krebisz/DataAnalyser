using System.Data;

namespace DataFileReader.Helper;

public static class DataHelper
{
    public static int GetNonEmptyFieldsTotal(DataTable dataTable)
    {
        var dataRow = dataTable.Rows[dataTable.Rows.Count - 1];

        var fieldTotal = 0;

        for (var j = 0; j < dataRow.ItemArray.Length; j++)
            if (!string.IsNullOrEmpty(dataRow[j].
                        ToString()))
                fieldTotal++;

        return fieldTotal;
    }

    public static int GetNonEmptyFieldsTotal(object?[] rowArray)
    {
        var fieldTotal = 0;

        for (var j = 0; j < rowArray.Length; j++)
            if (!string.IsNullOrEmpty(rowArray[j].
                        ToString()))
                fieldTotal++;

        return fieldTotal;
    }

    public static DataTable GetDistinctRows(DataTable sourceTable)
    {
        var maxfieldTotal = GetNonEmptyFieldsTotal(sourceTable);

        var newTable = sourceTable.Clone();

        var distinctRows = sourceTable.AsEnumerable().
                                       Select(row => row.ItemArray).
                                       Distinct(new RowComparer()).
                                       ToList(); // Apply the custom comparer

        foreach (var rowArray in distinctRows)
        {
            var fieldTotal = GetNonEmptyFieldsTotal(rowArray);

            if (fieldTotal >= maxfieldTotal)
                newTable.Rows.Add(rowArray); // Add each distinct row (as an array) to the new DataTable
        }

        return newTable;
    }


    // Custom comparer to treat nulls and empty strings as equal
    public class RowComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] x, object[] y)
        {
            if (x == null || y == null)
                return x == y; // Both are null, so they're equal

            // Compare each item in the row arrays
            for (var i = 0; i < x.Length; i++)
            {
                // Treat null and empty strings as equal
                if (string.IsNullOrEmpty(x[i]?.
                            ToString()) && string.IsNullOrEmpty(y[i]?.
                            ToString()))
                    continue;

                if (!Equals(x[i], y[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode(object[] obj)
        {
            // A simple hash code implementation, ensuring that rows with null or empty values have the same hash code
            var hash = 0;
            foreach (var item in obj)
                    // Null and empty values are treated as the same
                hash = hash * 31 + (item == null || string.IsNullOrEmpty(item.ToString()) ? 0 : item.GetHashCode());

            return hash;
        }
    }

    #region CharacterOperations

    public static string RemoveFaultyCharacterSequences(object stringObject)
    {
        var objectString = stringObject.ToString().
                                        Trim();

        objectString = objectString.Replace(",}", "}");
        objectString = objectString.Replace(",]", "]");

        return objectString;
    }

    public static string RemoveSpecialCharacters(string name)
    {
        char[] separator =
        {
                ',',
                '.'
        };
        var fileParts = name.Split(separator);
        name = fileParts.FirstOrDefault();

        var normalizedString = string.Empty;

        foreach (var character in name)
            if (!char.IsLetterOrDigit(character))
            {
            }
            else
            {
                normalizedString += character.ToString();
            }

        return normalizedString;
    }

    public static string RemoveEscapeCharacters(object stringObject)
    {
        var objectString = stringObject.ToString().
                                        Trim();

        objectString = objectString.Replace("\r", "");
        objectString = objectString.Replace("\n", "");
        objectString = objectString.Replace("\t", "");
        objectString = objectString.Replace(" ", "");

        return objectString;
    }

    #endregion CharacterOperations
}