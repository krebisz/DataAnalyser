namespace DataFileReader.Helper
{
    public class DataNormalization
    {
        public static void JSONPath(string jsonPath)
        {
            string[] JSONHLevels = jsonPath.Split('.');




            //DO YOU HAVE THE SAME SIBLINGS AS ME?, BUT A DIFFERNT PARENT? : WE NEED TO FALL IN LINE, AND CREATE A PARENT DEFINITION

            foreach (var item in JSONHLevels)
            {

            }
        }
    }
}
