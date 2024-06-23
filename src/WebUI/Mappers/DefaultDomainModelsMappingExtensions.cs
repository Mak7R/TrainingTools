using Domain.Models;
using WebUI.Models.ExerciseModels;

namespace WebUI.Mappers;

public static class DefaultDomainModelsMappingExtensions
{
    public static Exercise ToExercise(this AddExerciseModel addExerciseModel)
    {
        return new Exercise
        {
            Name = addExerciseModel.Name,
            Group = new Group
            {
                Id = addExerciseModel.GroupId
            },
            About = addExerciseModel.About
        };
    }

    public static Exercise ToExercise(this UpdateExerciseModel updateExerciseModel)
    {
        return new Exercise
        {
            Name = updateExerciseModel.Name,
            Group = new Group
            {
                Id = updateExerciseModel.GroupId
            },
            About = updateExerciseModel.About
        };
    }
    
}