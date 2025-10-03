namespace PegasusBackend.Helpers.ZoneHelper
{
    public static class ZoneHelper
    {
        public static bool ArlandaZone(string municipalite)
        {
            /* Problem med stockholm som kommun, för även globen/farsta
             * kommer ge stockholm som kommun även fast de är utanför zonen!
             * Behöver dela in den senare i norra och södra del av stockholm!
             */
            List<string> municipalities = new List<string>(new string[] 
            {
                "stockholm",
                "solna",
                "sundbyberg",
                "danderyd",
                "täby",
                "vallentuna",
                "sollentuna",
                "järfälla",
                "upplands väsby",
                "sigtuna",
                "knivsta",
                "uppsala"
            });

            municipalite = municipalite.ToLower().Trim();

            foreach (var municipalits in municipalities)
            {
                if (municipalite.Contains(municipalits))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
