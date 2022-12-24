using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlaySafe.Models;

namespace PlaySafe.Data
{
    public class dbContext : IdentityDbContext
    {
        public dbContext(DbContextOptions<dbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            string AdminRole = "864bee7c-01ac-448c-a886-88f2ef360c27";
            string OwnerRole = "c5f54ffb-5874-44eb-b216-1b6436530f72";
            string playerRole = "b0f6256a-00ca-4b14-81c3-b15a1a1401f7";
            string GuardRole = "1e367910-8fa0-405c-93e0-077f31de159f";
            string id = "64312d16-63ee-4851-9f84-10f1718a29f2";
            //seed admin role

            builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {

                Id = AdminRole,
                Name = "Admin",
                NormalizedName = "ADMIN",
            },
            new IdentityRole
            {
                Id = OwnerRole,
                Name = "Owner",
                NormalizedName = "OWNER",
            },
             new IdentityRole
             {
                 Id = playerRole,
                 Name = "Player",
                 NormalizedName = "PLAYER",
             },
              new IdentityRole
              {
                  Id = GuardRole,
                  Name = "Guard",
                  NormalizedName = "GUARD",
              }
            );
            builder.Entity<entry>().HasData(
                     new entry()
                     {
                         id = Guid.NewGuid(),
                         price = 10,
                         points = 20

                     },
                    new entry()
                    {
                        id = Guid.NewGuid(),
                        price = 5,
                        points = 10

                    },
                    new entry()
                    {
                        id = Guid.NewGuid(),
                        price = 20,
                        points = 40

                    }
);

            //create user

            var appUser = new ApplicationUser
            {
                Id = id,
                UserName = "Admin987",//DON'T REMOVE, username is the way
                name="DefaultAdmin"    ,                          //the user logs in in identity user
                                              //Amr//
                NormalizedUserName = "ADMIN987"
            };

            //set user password
            PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();
            appUser.PasswordHash = ph.HashPassword(appUser, "Ramez987");

            //seed user
            builder.Entity<ApplicationUser>().HasData(appUser);

            //set user role to admin

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = OwnerRole,
                UserId = id
            });
            base.OnModelCreating(builder);

        }

        public DbSet<ApplicationUser> user { get; set; }
        public DbSet<PlaySafe.Models.matchHistory> matchHistory { get; set; }
        
        //public DbSet<PlaySafe.Models.NFC> NFC { get; set; }
        public DbSet<PlaySafe.Models.specials> specials { get; set; }
        //public DbSet<PlaySafe.Models.comments> comments { get; set; }
        public DbSet<PlaySafe.Models.player> player { get; set; }
        //public DbSet<PlaySafe.Models.userTypePages> userTypePages { get; set; }
        public DbSet<PlaySafe.Models.entry> entry { get; set; }
        public DbSet<PlaySafe.Models.comments> comments { get; set; }
    }
}
