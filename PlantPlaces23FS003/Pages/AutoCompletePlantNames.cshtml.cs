using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Runtime.CompilerServices;

namespace PlantPlaces23FS003.Pages
{
    public class AutoCompletePlantNamesModel : PageModel
    {
        public JsonResult OnGet(String term)
        {
            IList<string> plantNames = new List<string>();
            plantNames.Add("Redbud");
            plantNames.Add("Red Maple");
            plantNames.Add("Red Oak");
            plantNames.Add("Red Rose");
            plantNames.Add("Red Lily");

            IList<string> matchingPlantNames = new List<String>();

            foreach(string plantName in plantNames)
            {
                if (plantName.Contains(term))
                {
                    matchingPlantNames.Add(plantName);
                }
            }

            return new JsonResult(matchingPlantNames);
        }
    }
}
