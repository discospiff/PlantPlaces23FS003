using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
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
            // add this to our in memory repository
            SpecimenRepository.allSpecimens = specimens;
        }

        /// <summary>
        /// Access APIs that have the data we are combining.
        /// </summary>
        /// <returns>A list of specimens that are thirsty.</returns>
        private async Task<List<Specimen>> GetSpecimenData () {
            return await Task.Run(async () =>
            {
                Task<HttpResponseMessage> plantTask = httpClient.GetAsync("https://plantplaces.com/perl/mobile/viewplantsjsonarray.pl?WetTolerant=on");

                Task<HttpResponseMessage> specimenTask = httpClient.GetAsync("https://plantplaces.com/perl/mobile/specimenlocations.pl?Lat=39.1455&Lng=-84.509&Range=0.5&Source=location");

                // grab the API key from THE SECRET STORE
                var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
                string weatherApiKey = config["weatherapikey"];
                string weatherEndpoint = "https://api.weatherbit.io/v2.0/current?&city=Cincinnati&country=USA&key=" + weatherApiKey;
                Task<HttpResponseMessage> weatherTask = httpClient.GetAsync(weatherEndpoint);


                HttpResponseMessage response = specimenTask.Result;
                List<Specimen> specimens = new List<Specimen>();

                if (response.IsSuccessStatusCode)
                {
                    Task<string> readString = response.Content.ReadAsStringAsync();
                    string specimenJson = readString.Result;

                    // read in the schema file
                    JSchema schema = JSchema.Parse(System.IO.File.ReadAllText("specimenschema.json"));
                    // perform a very simple JSON parse into an array.
                    JArray specimenArray = JArray.Parse(specimenJson);
                    // create a collection of Strings that will hold any validation errors.
                    IList<string> validationEvents = new List<string>();

                    if(specimenArray.IsValid(schema, out validationEvents))
                    {
                        specimens = Specimen.FromJson(specimenJson);
                    } 
                    else
                    {
                        foreach (string evt in validationEvents)
                        {
                            Console.WriteLine(evt);
                        }

                    }
                 
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

                // get weather data from the async call to weather service.
                HttpResponseMessage weatherResponse = await weatherTask;
                Task<string> weatherReadTask = weatherResponse.Content.ReadAsStringAsync();
                string weatherJson = weatherReadTask.Result;

                // parse JSON weather to objects, using QuickType.
                WeatherFeed.Weather weather = WeatherFeed.Weather.FromJson(weatherJson);
                
                // get our weather data, from which we can get individual data items.
                List<WeatherFeed.Datum> weatherData = weather.Data;

                // assume precip is 0, until we know what it is.
                long precip = 0;

                // iterate over the data items, until we find the weather attribute we're looking for.
                foreach (WeatherFeed.Datum datum in weatherData)
                {
                    // if we know acutal precip, assign it here.
                    precip = datum.Precip;
                }

                if (precip < 1)
                {
                    ViewData["WeatherMessage"] = "It's dry!  Water these plants.";
                } else
                {
                    ViewData["WeatherMessage"] = "Rain expected.  No need to water.";
                }

                return waterLovingSpecimens;
            });
        }
    }
}