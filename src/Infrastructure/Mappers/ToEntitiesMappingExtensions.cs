﻿using Domain.Models.TrainingPlan;
using Infrastructure.Entities.TrainingPlanEntities;

namespace Infrastructure.Mappers;

public static class ToEntitiesMappingExtensions
{
    public static TrainingPlanEntity ToTrainingPlanEntity(this TrainingPlan trainingPlan, bool generateId)
    {
        int i = 0;
        return new TrainingPlanEntity
        {
            Id = trainingPlan.Id,
            Author = trainingPlan.Author,
            Name = trainingPlan.Name,
            IsPublic = trainingPlan.IsPublic,
            TrainingPlanBlocks = trainingPlan.TrainingPlanBlocks.Select(b => b.ToTrainingPlanBlockEntity(i++, generateId))
        };
    }

    public static TrainingPlanBlockEntity ToTrainingPlanBlockEntity(this TrainingPlanBlock trainingPlanBlock, int position, bool generateId)
    {
        int i = 0;
        return new TrainingPlanBlockEntity
        {
            Id = generateId ? Guid.NewGuid() : Guid.Empty,
            Name = trainingPlanBlock.Name,
            Position = position,
            TrainingPlanBlockEntries = trainingPlanBlock.TrainingPlanBlockEntries.Select(e => e.ToTrainingPlanBlockEntryEntity(i++, generateId))
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