using Microsoft.EntityFrameworkCore;
using OmniChat.Domain.MCP;

namespace OmniChat.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<McpMessage>(entity =>
        {
            entity.OwnsOne(e => e.Content, cb =>
            {
                cb.Property(c => c.CipherText).HasColumnName("Message_Content");
                cb.Property(c => c.IV).HasColumnName("Message_IV");
            });
        });
    }
}