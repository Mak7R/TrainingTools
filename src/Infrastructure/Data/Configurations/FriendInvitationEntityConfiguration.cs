using Infrastructure.Entities.Friendship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class FriendInvitationEntityConfiguration : IEntityTypeConfiguration<FriendInvitationEntity>
{
    public void Configure(EntityTypeBuilder<FriendInvitationEntity> builder)
    {
        builder
            .ToTable("FriendInvitation")
            .HasKey(fi => new { fi.InvitorId, TargetId = fi.InvitedId });
        

        builder
            .HasOne(fi => fi.Invitor)
            .WithMany()
            .HasForeignKey(fi => fi.InvitorId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder
            .HasOne(fi => fi.Invited)
            .WithMany()
            .HasForeignKey(fi => fi.InvitedId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}