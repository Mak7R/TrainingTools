namespace Domain.Rules;

public static class DataSizes
{
    public static class ApplicationUserDataSizes
    {
        public const int MaxUsernameSize = 64;
        public const int MinUsernameSize = 4;

        public const int MaxEmailSize = 320;
        
        public const int MaxAboutSize = 1024;
    }
    
    public static class RoleDataSizes
    {
        
    }

    public static class GroupDataSizes
    {
        public const int MaxNameSize = 64;
        public const int MinNameSize = 4;
    }
    
    public static class ExerciseDataSizes
    {
        public const int MaxNameSize = 64;
        public const int MinNameSize = 4;
        public const int MaxAboutSize = 2048;
    }
    
    public static class ExerciseResultsSizes
    {
        public const int MaxWeightsSize = 144;
        public const int MaxCountsSize = 64;
        public const int MaxCommentsSize = 512;
    }
    
    public static class TrainingPlanDataSizes
    {
        public const int MaxNameSize = 64;
        public const int MaxBlockNameSize = 32;
        public const int MaxBlockEntryDescriptionSize = 128;
    }
}