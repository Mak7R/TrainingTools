namespace Application.Constants;

public static class FilterOptionNames
{
    public static class Group
    {
        public const string Name = "name";
    } 
    
    public static class Exercise
    {
        public const string Name = "name";
        public const string Group = "group";
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
            public const string FullName = "full-name";
        }
        
        public static class ForExercise
        {
            public const string OwnerName = "owner";
        }
    }
}