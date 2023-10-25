using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlantPlacesPlants;
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
            Task<List<Specimen>> task = GetSpecimenData();
            List<Specimen> specimens = task.Result;
            ViewData["Specimens"] = specimens;
        }

        private async Task<List<Specimen>> GetSpecimenData () {
            return await Task.Run(async () =>
            {
                Task<HttpResponseMessage> plantTask = httpClient.GetAsync("https://plantplaces.com/perl/mobile/viewplantsjsonarray.pl?WetTolerant=on");

                Task<HttpResponseMessage> task = httpClient.GetAsync("https://plantplaces.com/perl/mobile/specimenlocations.pl?Lat=39.1455&Lng=-84.509&Range=0.5&Source=location");
                HttpResponseMessage response = task.Result;
                List<Specimen> specimens = new List<Specimen>();
                if (response.IsSuccessStatusCode)
                {
                    Task<string> readString = response.Content.ReadAsStringAsync();
                    string specimenJson = readString.Result;
                    specimens = Specimen.FromJson(specimenJson);
                }

                HttpResponseMessage plantResponse = await plantTask;
                Task<string> plantStringTask = plantResponse.Content.ReadAsStringAsync();
                string plantsJson = plantStringTask.Result;
                List<Plant> plants = Plant.FromJson(plantsJson);

                IDictionary<long, Plant> waterLovingPlants = new Dictionary<long, Plant>();
                foreach (Plant plant in plants)
                {
                    waterLovingPlants[plant.Id] = plant;
                }
                List<Specimen> waterLovingSpecimens = new List<Specimen>();
                foreach (Specimen specimen in specimens)
                {
                    if (waterLovingPlants.ContainsKey(specimen.PlantId))
                    {
                        waterLovingSpecimens.Add(specimen);
                    }
                }
                return waterLovingSpecimens;
            });
        }
    }
}