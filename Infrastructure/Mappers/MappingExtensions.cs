using Domain.Models;
using Domain.Rules;
using Infrastructure.Entities;

namespace Infrastructure.Mappers;

public static class MappingExtensions
{
    public static Group ToGroup(this GroupEntity groupEntity)
    {
        return new Group
        {
            Id = groupEntity.Id, 
            Name = groupEntity.Name
        };
    }

    public static Exercise ToExercise(this ExerciseEntity exerciseEntity)
    {
        return new Exercise
        {
            Id = exerciseEntity.Id, 
            Name = exerciseEntity.Name, 
            Group = exerciseEntity.Group.ToGroup()
        };
    }

    public static ExerciseResult ToExerciseResult(this ExerciseResultEntity exerciseResultEntity)
    {
        var weights = exerciseResultEntity.Weights?.Split(SpecialConstants.DefaultSeparator).Select(decimal.Parse).ToArray() ?? [];
        var counts = exerciseResultEntity.Counts?.Split(SpecialConstants.DefaultSeparator).Select(int.Parse).ToArray() ?? [];
        var comments = exerciseResultEntity.Comments?.Split(SpecialConstants.DefaultSeparator).ToArray() ?? [];

        var approachInfos = weights.Select((t, i) => new Approach(t, counts[i], comments[i])).ToList();

        return new ExerciseResult
        {
            Owner = exerciseResultEntity.Owner,
            Exercise = exerciseResultEntity.Exercise.ToExercise(),
            ApproachInfos = approachInfos
        };
    }
}