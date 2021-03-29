using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseTextToJson
{
    class TableOfContent
    {
        private int volume;
        private string characterVersion;
        private string theme;
        private int year;
        private int month;
        private Dictionary<string, List<ArticleInfo>> tableOfContent = new Dictionary<string, List<ArticleInfo>>();
        private Dictionary<int, string> indexToCategory = new Dictionary<int, string>();

        public TableOfContent()
        {
        }

        public void SetInfo(int volume, string characterVersion, string theme, int year, int month)
        {
            this.volume = volume;
            this.characterVersion = characterVersion;
            this.theme = theme;
            this.year = year;
            this.month = month;
        }

        // Add new ArticleInfo to table of content dictionary based on its category.
        public void AddNewArticleInfo(ArticleInfo articleInfo, int index)
        {
            // Error checking
            if (articleInfo == null)
            {
                return;
            }

            // Check if articleInfo's category is already defined in the dictionary
            if (tableOfContent.ContainsKey(articleInfo.Category))
            {
                // if so, add to the list of that category
                tableOfContent[articleInfo.Category].Add(articleInfo);
            }
            else
            {
                // if not, create a new empty list, add the list in the dictionary, and
                // add the new article info in that list
                tableOfContent[articleInfo.Category] = new List<ArticleInfo>() { articleInfo };
            }

            indexToCategory[index] = articleInfo.Category;
        }

        private List<string> GetOrderedCategoryList(Dictionary<int, string> indexToCategory)
        {
            List<string> orderedCategoryList = new List<string>();

            int[] keys = indexToCategory.Keys.ToArray();
            Array.Sort(keys);
            foreach (int key in keys)
            {
                string category = indexToCategory[key];
                if (orderedCategoryList.LastOrDefault() != category)
                {
                    orderedCategoryList.Add(category);
                }
            }

            return orderedCategoryList;
        }

        // Generate JSON based on info given
        public void GenerateJSON(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("{");
                sw.WriteLine("    \"volume\": \"" + volume + "\",");
                sw.WriteLine("    \"title\": \"溪水旁第" + volume + "期\",");
                sw.WriteLine("    \"character\": \"" + characterVersion + "\",");
                if (theme != "")
                {
                    sw.WriteLine("    \"theme\": \"" + theme + "\",");
                }

                if (year != 0)
                {
                    sw.WriteLine("    \"year\": \"" + year + "\",");
                }

                if (month != 0)
                {
                    sw.WriteLine("    \"month\": \"" + month + "\",");
                }

                sw.WriteLine("    \"table_of_content\": [");

                List<string> lstCategories = GetOrderedCategoryList(indexToCategory);

                foreach (string category in lstCategories)
                {
                    sw.WriteLine("        {");
                    sw.WriteLine("            \"category\": \"" + category + "\",");
                    sw.WriteLine("            \"articles\": [");

                    List<ArticleInfo> lstArticles = tableOfContent[category];

                    foreach (ArticleInfo articleInfo in lstArticles)
                    {
                        sw.WriteLine("                {");
                        sw.WriteLine("                    \"title\": \"" + articleInfo.Title + "\",");
                        sw.WriteLine("                    \"author\": \"" + articleInfo.Author + "\",");
                        sw.WriteLine("                    \"id\": \"" + articleInfo.ID + "\"");
                        sw.WriteLine("                }");
                        if (articleInfo != lstArticles.Last())
                        {
                            sw.WriteLine("                ,");
                        }
                    }

                    sw.WriteLine("            ]");
                    sw.WriteLine("        }");

                    if (category != lstCategories.Last())
                    {
                        sw.WriteLine("        ,");
                    }
                }

                sw.WriteLine("    ]");
                sw.WriteLine("}");
            }
        }

    }
}
