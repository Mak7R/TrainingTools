using Domain.Models.TrainingPlan;
using Infrastructure.Entities.TrainingPlanEntities;

namespace Infrastructure.Mapping.Mappers;

public static class ToEntitiesMappingExtensions
{
    public static TrainingPlanEntity ToTrainingPlanEntity(this TrainingPlan trainingPlan, bool generateId)
    {
        int i = 0;
        return new TrainingPlanEntity
        {
            Id = trainingPlan.Id,
            Author = trainingPlan.Author,
            Title = trainingPlan.Title,
            IsPublic = trainingPlan.IsPublic,
            TrainingPlanBlocks = trainingPlan.TrainingPlanBlocks.Select(b => b.ToTrainingPlanBlockEntity(i++, generateId)).ToList()
        };
    }

    public static TrainingPlanBlockEntity ToTrainingPlanBlockEntity(this TrainingPlanBlock trainingPlanBlock, int position, bool generateId)
    {
        int i = 0;
        return new TrainingPlanBlockEntity
        {
            Id = generateId ? Guid.NewGuid() : Guid.Empty,
            Title = trainingPlanBlock.Name,
            Position = position,
            TrainingPlanBlockEntries = trainingPlanBlock.TrainingPlanBlockEntries.Select(e => e.ToTrainingPlanBlockEntryEntity(i++, generateId)).ToList()
        };
    }

    public static TrainingPlanBlockEntryEntity ToTrainingPlanBlockEntryEntity(this TrainingPlanBlockEntry trainingPlanBlockEntry, int position, bool generateId)
    {
        return new TrainingPlanBlockEntryEntity
        {
            Id = generateId ? Guid.NewGuid() : Guid.Empty,
            GroupId = trainingPlanBlockEntry.Group.Id,
            Desctiption = trainingPlanBlockEntry.Description,
            Position = position
        };
    }
}