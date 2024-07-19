using Domain.Models;
using Domain.Models.TrainingPlan;
using Domain.Rules;
using Infrastructure.Entities;
using Infrastructure.Entities.TrainingPlanEntities;

namespace Infrastructure.Mapping.Mappers;

public static class ToDomainModelsMappingExtensions
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
            Group = exerciseEntity.Group.ToGroup(),
            About = exerciseEntity.About
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

    public static TrainingPlan ToTrainingPlan(this TrainingPlanEntity trainingPlanEntity)
    {
        return new TrainingPlan
        {
            Id = trainingPlanEntity.Id,
            Title = trainingPlanEntity.Title,
            IsPublic = trainingPlanEntity.IsPublic,
            Author = trainingPlanEntity.Author,
            TrainingPlanBlocks = trainingPlanEntity.TrainingPlanBlocks
                .OrderBy(b => b.Position)
                .Select(b => b.ToTrainingPlanBlock())
                .ToList()
        };
    }

    public static TrainingPlanBlock ToTrainingPlanBlock(this TrainingPlanBlockEntity trainingPlanBlockEntity)
    {
        return new TrainingPlanBlock
        {
            Name = trainingPlanBlockEntity.Title,
            TrainingPlanBlockEntries = trainingPlanBlockEntity.TrainingPlanBlockEntries
                .OrderBy(e => e.Position)
                .Select(e => e.ToTrainingPlanBlock())
                .ToList()
        };
    }
    
    public static TrainingPlanBlockEntry ToTrainingPlanBlock(this TrainingPlanBlockEntryEntity trainingPlanBlockEntryEntity)
    {
        return new TrainingPlanBlockEntry
        {
            Group = trainingPlanBlockEntryEntity.Group.ToGroup(),
            Description = trainingPlanBlockEntryEntity.Desctiption
        };
    }
}