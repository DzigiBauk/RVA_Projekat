using Komponenta1.Models;

namespace Komponenta1.Interfaces;

public interface IValidator<in T>
{
    ValidationResult Validate(T value, Guid? existingEntityId = null);
}
