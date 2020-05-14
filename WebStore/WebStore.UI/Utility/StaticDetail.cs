namespace WebStore.UI.Utility
{
    public static class StaticDetail
    {
        public const string DefaultTeaPictureUri = "default_tea.png";

        public const string ManagerUser = "Manager";
        public const string SupplyUser = "Supply";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustomerEndUser = "Customer";

        public const string startSessionShoppingCartCount = "startSessionCartCount";

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
} 