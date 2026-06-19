using Shared.Models;

namespace Shared.Contracts;

[CoreWCF.ServiceContract]
[System.ServiceModel.ServiceContract]
public interface IAquariumService
{
    [CoreWCF.OperationContract]
    [System.ServiceModel.OperationContract]
    Task<List<AquaticSpecies>> GetSpeciesAsync();

    [CoreWCF.OperationContract]
    [System.ServiceModel.OperationContract]
    Task<List<WaterQualityReading>> GetReadingsAsync(
        ReadingsRequest request);
}
