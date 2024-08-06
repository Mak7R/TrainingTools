using Domain.Models;

namespace Application.Interfaces.Services;

public interface IExerciseResultsToExсelExporter
{
    public Task<Stream> ToExсel(IEnumerable<ExerciseResult> results);
}