namespace Domain.Enums;

public enum Role
{
    User, // has common user rights
    Admin, // has rights to edit groups and exercises
    Root, // has rights to full control server (delete users, edit users, add users (admins))
    Trainer // special role for trainers it shows that the user is verified trainer by admins
}