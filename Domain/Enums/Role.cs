namespace Domain.Enums;

public enum Role
{
    User, // has common user rights
    Admin, // has rights to edit groups and exercises
    Root // has rights to full control server (delete users, edit users, add users (admins))
}