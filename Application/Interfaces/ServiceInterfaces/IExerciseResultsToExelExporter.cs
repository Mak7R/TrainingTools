using Domain.Models;

namespace Application.Interfaces.ServiceInterfaces;

public interface IExerciseResultsToExelExporter
{ 
    public Task<Stream> ToExel(IEnumerable<ExerciseResult> results);
}