namespace Application.Constants;

public static class OrderOptionNames
{
    public static class Shared
    {
        /// <summary>
        ///     Ascending order
        /// </summary>
        public const string Ascending = "asc";

        /// <summary>
        ///     Descending order
        /// </summary>
        public const string Descending = "desc";

        /// <summary>
        ///     Unordered
        /// </summary>
        public const string Empty = "";


        /// <summary>
        ///     Name of query value which represents an order method (like ascending or etc.)
        /// </summary>
        public const string Order = "order";

        /// <summary>
        ///     Name of query value which represents an element which value is used for ordering
        /// </summary>
        public const string OrderBy = "order_by";
    }

    public static class Group
    {
        /// <summary>
        ///     Order by name
        /// </summary>
        public const string Name = "name";
    }

    public static class Exercise
    {
        /// <summary>
        ///     Order by name
        /// </summary>
        public const string Name = "name";

        /// <summary>
        ///     Order by group name
        /// </summary>
        public const string GroupName = "group-name";
    }

    public static class User
    {
        /// <summary>
        ///     Order by name
        /// </summary>
        public const string Name = "name";

        /// <summary>
        ///     Order by role
        /// </summary>
        public const string Role = "role";

        /// <summary>
        ///     Order by relationships
        /// </summary>
        public const string RelationshipsState = "relationships";
    }

    public static class ExerciseResults
    {
        /// <summary>
        ///     Order by group name
        /// </summary>
        public const string GroupName = "group-name";


        /// <summary>
        ///     Order by owner name
        /// </summary>
        public const string OwnerName = "owner";
    }

    public static class TrainingPlan
    {
        /// <summary>
        ///     Order by title
        /// </summary>
        public const string Title = "title";

        /// <summary>
        ///     Order by author name
        /// </summary>
        public const string AuthorName = "owner";
    }

    public static class Relationships
    {
        public static class FriendInvitation
        {
            /// <summary>
            ///     Order by invitation date time
            /// </summary>
            public const string InvitationDateTime = "date";
        }

        public static class Friendship
        {
            /// <summary>
            ///     Order by  date time
            /// </summary>
            public const string FriendsFromDateTime = "date";
        }
    }
}