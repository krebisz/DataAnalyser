using System.Data;
using System.Text;
using static DataFileReader.Helper.DataHelper;

namespace DataFileReader.Class;

public class MetaDataList
{
    public List<string> ElementsList = new();

    public MetaDataList()
    {
        MetaDataObjects = new List<MetaData>();
    }

    public MetaDataList(HierarchyObjectList HierarchyObjectList)
    {
        MetaDataObjects = new List<MetaData>();

        HierarchyObjectList.GenerateMetaIDs();

        foreach (var hierarchyObject in HierarchyObjectList.HierarchyObjects)
        {
            //hierarchyObject.ReferenceValue = GetMetaDataObjectReferenceValue(HierarchyObjectList.HierarchyObjects, hierarchyObject.ID, ref referenceValue).ToString();
            hierarchyObject.ReferenceValue = GetMetaDataObjectReferenceValue(HierarchyObjectList.HierarchyObjects, hierarchyObject.ID);
            if (string.IsNullOrEmpty(hierarchyObject.Name))
                hierarchyObject.Name = Guid.NewGuid().ToString();

            var metaData = new MetaData();

            var type = hierarchyObject.Value.GetType();

            metaData.Fields.Add(hierarchyObject.Value, type);
            metaData.GenerateID();

            metaData.Name = hierarchyObject.Name;
            metaData.Type = hierarchyObject.ClassID;
            metaData.ReferenceValue = hierarchyObject.ReferenceValue;

            if (metaData.Type != "Element")
            {
                var existingMetaData = MetaDataObjects.FirstOrDefault(x => x.ReferenceValue == metaData.ReferenceValue);

                //if (existingMetaData is null)
                //{
                MetaDataObjects.Add(metaData);
                //}
            }
            else
            {
                var existingElement = ElementsList.FirstOrDefault(x => x == metaData.Name);

                if (existingElement is null)
                    ElementsList.Add(metaData.Name);
            }
        }
    }

    public List<MetaData> MetaDataObjects { get; set; }

    public DataTable FlattenData(HierarchyObjectList hierarchyObjectList)
    {
        return FlattenData(hierarchyObjectList.HierarchyObjects);
    }

    public DataTable FlattenData(List<HierarchyObject> hierarchyObjects)
    {
        var flattenedData = new DataTable();

        if (ElementsList != null && ElementsList.Count > 0)
            for (var i = 0; i < ElementsList.Count; i++)
            {
                var column = new DataColumn(ElementsList.ElementAt(i), typeof(string));
                flattenedData.Columns.Add(column);
            }

        var dataFields = new string[flattenedData.Columns.Count];
        var currentDataFields = new string[flattenedData.Columns.Count];

        //HIERARCHYOBJECTS SHOULD BE GUARANTEED TO BE SORTED BEFORE THE FOLLOWING:
        foreach (var hierarchyObject in hierarchyObjects)
        {
            var flattenedDataRow = flattenedData.NewRow();

            if (hierarchyObject.ClassID == "Element")
            {
                var flattenedDataColumn = flattenedData.Columns.Cast<DataColumn>().SingleOrDefault(col => col.ColumnName == hierarchyObject.Name);

                if (flattenedDataColumn != null)
                    flattenedDataRow[flattenedDataColumn] = hierarchyObject.Value;

                flattenedData.Rows.Add(flattenedDataRow);
            }
        }

        for (var i = 0; i < flattenedData.Rows.Count; i++)
        {
            for (var j = 0; j < flattenedData.Columns.Count; j++)
            {
                currentDataFields[j] = flattenedData.Rows[i][j]?.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(currentDataFields[j]))
                {
                    dataFields[j] = currentDataFields[j];
                }
                else
                {
                    currentDataFields[j] = dataFields[j];
                    flattenedData.Rows[i][j] = currentDataFields[j];
                }
            }
        }

        var distinctTable = GetDistinctRows(flattenedData);

        return distinctTable;
    }

    public static int? GetMetaDataObjectReferenceValue(List<HierarchyObject> HierarchyObjectList, int hierarchyObjectID, ref int? referenceValue)
    {
        if (referenceValue == null)
            referenceValue = 0;

        var hierarchyObject = HierarchyObjectList.FirstOrDefault(x => x.ID == hierarchyObjectID);

        if (hierarchyObject != null)
        {
            referenceValue = referenceValue + hierarchyObject.MetaDataID;

            if (hierarchyObject.ParentID != null && hierarchyObject.ParentID > -1)
                GetMetaDataObjectReferenceValue(HierarchyObjectList, (int)hierarchyObject.ParentID, ref referenceValue);
        }

        return referenceValue;
    }

    public static string GetMetaDataObjectReferenceValue(List<HierarchyObject> HierarchyObjectList, int ID)
    {
        var referenceValue = string.Empty;
        var reference = new StringBuilder();

        var hierarchyObject = HierarchyObjectList.FirstOrDefault(x => x.ID == ID);
        var hierarchyObjectParent = HierarchyObjectList.FirstOrDefault(x => x.ID == hierarchyObject?.ParentID);

        if (hierarchyObjectParent != null && hierarchyObjectParent.ReferenceValue != null)
            reference.Append(hierarchyObjectParent.ReferenceValue);
        if (hierarchyObject != null)
            reference.Append(hierarchyObject.ID);

        return reference.ToString();
    }
}