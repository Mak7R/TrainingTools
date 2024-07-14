namespace Application.Constants;

public static class OrderOptionNames
{
    public static class Shared
    {
        public const string Ascending = "asc";
        public const string Descending = "desc";
    }
    
    public static class Group
    {
        public const string Name = "name";
    }
    
    public static class Exercise
    {
        public const string Name = "name";
        public const string GroupName = "group-name";
    }
    
    public static class User
    {
        public const string Name = "name";
        public const string Role = "role";
        public const string FriendshipState = "friendship";
    }
    
    public static class ExerciseResults
    {
        public static class ForUser
        {
            public const string GroupName = "group-name";
        }
        
        public static class ForExercise
        {
            public const string OwnerName = "owner";
        }
    }
    
    public static class TrainingPlan
    {
        public const string Title = "title";
        public const string Author = "owner";
    }
}