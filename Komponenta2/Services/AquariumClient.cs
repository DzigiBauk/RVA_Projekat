using Komponenta2.Interfaces;
using Shared.Contracts;
using Shared.Models;

namespace Komponenta2.Services
{
    public class AquariumClient : IAquariumClient
    {
        private readonly IAquariumService proxy;

        public AquariumClient(IAquariumService proxy)
        {
            this.proxy = proxy;
        }

        public async Task<List<WaterQualityReading>> GetReadings(Guid speciesId, int month)
        {
            var request = new ReadingsRequest
            {
                SpeciesId = speciesId,
                Month = month
            };

            return await proxy.GetReadingsAsync(request);
        }

        public async Task<List<AquaticSpecies>> GetSpecies()
        {
            return await proxy.GetSpeciesAsync();
        }   
    }
}
