﻿using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TrainingTools.Models;

public class AddExerciseModel
{
    [Display(Name = "Exercise name")]
    [Required(ErrorMessage = "Name cannot be empty")]
    [StringLength(Exercise.MaxNameLength, ErrorMessage = "Exercise name invalid length")]
    public string Name { get; set; }
    
    [Display(Name = "Group")]
    public Guid? GroupId { get; set; }
    
    [BindNever]
    public IEnumerable<GroupViewModel> Groups { get; set; }
}