using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Serialization;
using System.Text.Json;
using Contracts.Exceptions;

namespace Contracts.Models;

public class ExerciseResults
{
    public const int MaxResultsJsonStringLength = 128_000;
    
    public Guid Id { get; set; }
    
    [ForeignKey(nameof(Owner))]
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
    
    [ForeignKey(nameof(Exercise))]
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; }

    [StringLength(MaxResultsJsonStringLength)]
    public string ResultsJson { get; set; }
}