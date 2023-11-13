using PlantPlacesSpecimens;

namespace PlantPlaces23FS003
{
    public class SpecimenRepository
    {

        static SpecimenRepository()
        {
            allSpecimens = new List<Specimen>();
        }

        public static IList<Specimen> allSpecimens { get; set; } 
    }
}
