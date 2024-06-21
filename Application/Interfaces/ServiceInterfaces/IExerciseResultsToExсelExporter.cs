using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IExerciseResultsToExсelExporter
{ 
    public Task<Stream> ToExсel(IEnumerable<ExerciseResult> results);
}