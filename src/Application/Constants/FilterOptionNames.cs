namespace Application.Constants;

public static class FilterOptionNames
{
    public static class Shared
    {
        /// <summary>
        /// This value is used for detecting filters in request query
        /// </summary>
        public const string FiltersPrefix = "f_";

        public const string MultiplyFilterValuesSeparator = ",";
    }
    public static class Group
    {
        /// <summary>
        /// Name contains value
        /// </summary>
        public const string Name = "name";

        /// <summary>
        /// Name equals value
        /// </summary>
        public const string NameEquals = "name-equals";
    } 
    
    public static class Exercise
    {
        /// <summary>
        /// Name contains value
        /// </summary>
        public const string Name = "name";
        
        /// <summary>
        /// Name equals value
        /// </summary>
        public const string NameEquals = "name-equals";
        
        /// <summary>
        /// Has same group (groupId) as value
        /// </summary>
        public const string GroupId = "group";
    }
    
    public static class User
    {
        /// <summary>
        /// Name contains value
        /// </summary>
        public const string Name = "name";
        
        /// <summary>
        /// Name equals value
        /// </summary>
        public const string NameEquals = "name-equals";
        
        /// <summary>
        /// Has role
        /// </summary>
        public const string Role = "role";
        
        /// <summary>
        /// User relationships to user
        /// </summary>
        public const string RelationshipsState = "relationships";
    }
    
    public static class ExerciseResults
    {
        /// <summary>
        /// Exercise full name contains value
        /// </summary>
        public const string FullName = "full-name";
        
        /// <summary>
        /// Exercise full name equals value
        /// </summary>
        public const string FullNameEquals = "full-name-equals";
        
        /// <summary>
        /// Owner name contains value
        /// </summary>
        public const string OwnerName = "owner";
        
        /// <summary>
        /// Owner name equals value
        /// </summary>
        public const string OwnerNameEquals = "owner-equals";
    }
    
    public static class TrainingPlan
    {
        /// <summary>
        /// Title contains value
        /// </summary>
        public const string Title = "title";
        
        /// <summary>
        /// Title equals value
        /// </summary>
        public const string TitleEquals = "title-equals";
        
        /// <summary>
        /// Author id is the same as value
        /// </summary>
        public const string AuthorId = "author-id";
        
        /// <summary>
        /// Author name contains
        /// </summary>
        public const string AuthorName = "author";
        
        /// <summary>
        /// Author name equals
        /// </summary>
        public const string AuthorNameEquals = "author-equals";
        
        
        /// <summary>
        /// If Public Only value equals to true returns only public values
        /// </summary>
        public const string PublicOnly = "public-only";
    }
    
    public static class Relationships
    {
        public static class FriendInvitation
        {
            /// <summary>
            /// Invitor id is the same as value
            /// </summary>
            public const string Invitor = "invitor-id";
            
            /// <summary>
            /// Invited id is the same as value
            /// </summary>
            public const string Invited = "invited-id";
        }

        public static class Friendship
        {
            /// <summary>
            /// Friend id is the same as value
            /// </summary>
            public const string FriendId = "friend-id";
        }
    }
}