namespace Domain.Rules;

public static class DataSizes
{
    public static class ApplicationUser
    {
        public const int MaxUsernameSize = 32;
        public const int MinUsernameSize = 4;

        public const int MaxAboutSize = 1024;

        public const int MaxEmailSize = 320;
    }
    
    public static class Role
    {
        
    }

    public static class Group
    {
        public const int MaxNameSize = 32;
        public const int MinNameSize = 4;
    }
    
    public static class Exercise
    {
        public const int MaxNameSize = 32;
        public const int MinNameSize = 4;
        public const int MaxAboutSize = 2048;
    }
    
    public static class ExerciseResults
    {
        public const int MaxWeightsSize = 144;
        public const int MaxCountsSize = 64;
        public const int MaxCommentsSize = 512;
    }
}