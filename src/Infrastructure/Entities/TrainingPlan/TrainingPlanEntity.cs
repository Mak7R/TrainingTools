﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Identity;
using static Domain.Rules.DataSizes.TrainingPlanDataSizes;


namespace Infrastructure.Entities.TrainingPlan;

public class TrainingPlanEntity
{
    [Key] public Guid Id { get; set; }

    [StringLength(MaxTitleSize)] public string Title { get; set; } = string.Empty;

    [ForeignKey(nameof(Author))] public Guid AuthorId { get; set; }

    public ApplicationUser Author { get; set; }

    public bool IsPublic { get; set; }

    public IList<TrainingPlanBlockEntity> TrainingPlanBlocks { get; set; } = new List<TrainingPlanBlockEntity>();
}