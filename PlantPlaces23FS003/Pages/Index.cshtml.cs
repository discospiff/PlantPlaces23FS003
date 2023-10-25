using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantPlacesSpecimens;

namespace PlantPlaces23FS003.Pages
{
    public class IndexModel : PageModel
    {
        static readonly HttpClient httpClient = new HttpClient();

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string brand = "My Plant Diary";
            string inBrand = Request.Query["Brand"];
            if (inBrand != null && inBrand.Length > 0)
            {
                // if a brand was passed in, use it.  Otherwise, use default.
                brand = inBrand;
            }
            ViewData["Brand"] = brand;

            Task<HttpResponseMessage> task = httpClient.GetAsync("https://plantplaces.com/perl/mobile/specimenlocations.pl?Lat=39.1455&Lng=-84.509&Range=0.5&Source=location");
            HttpResponseMessage response = task.Result;
            List<Specimen> specimens = new List<Specimen>();
            if (response.IsSuccessStatusCode)
            {
                Task<string> readString = response.Content.ReadAsStringAsync();
                string specimenJson = readString.Result;
                specimens = Specimen.FromJson(specimenJson);
            }
            ViewData["Specimens"] = specimens;
        }
    }
}