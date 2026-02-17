using System;
using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{

    
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);  
            
            entity.Property(e => e.firstLine).IsRequired();
            entity.Property(e => e.city).IsRequired();
            entity.Property(e => e.state).IsRequired();
            entity.Property(e => e.postalCode).IsRequired();
            entity.Property(e => e.country).IsRequired();
        });
        
       
    }
    

}
