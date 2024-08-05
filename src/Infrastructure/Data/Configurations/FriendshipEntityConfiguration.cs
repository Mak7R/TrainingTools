using Infrastructure.Entities.Friendship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class FriendshipEntityConfiguration : IEntityTypeConfiguration<FriendshipEntity>
{
    public void Configure(EntityTypeBuilder<FriendshipEntity> builder)
    {
        builder
            .ToTable("Friendship")
            .HasKey(fr => new { fr.FirstFriendId, fr.SecondFriendId });
        
        builder
            .HasOne(fr => fr.FirstFriend)
            .WithMany()
            .HasForeignKey(fr => fr.FirstFriendId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder
            .HasOne(fr => fr.SecondFriend)
            .WithMany()
            .HasForeignKey(fr => fr.SecondFriendId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}