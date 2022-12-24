using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlaySafe.Models;
using System.Security.Cryptography;
using System.Text;

namespace PlaySafe.Data
{
    public class AppDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<dbContext>();
                if (context == null) return;
                if (!context.entry.Any())
                {
                    context.entry.AddRange(
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
                    context.SaveChanges();
                }
                /*
                                if (!context.user.Any())
                                {
                                    var type = context.userType.Where(c => c.usersType == "Owner");
                                    var user = new PlaySafe.Models.ApplicationUser()
                                    {
                                        name = "defaultOwner",
                                        UserName = "Admin",
                                        createdDate = DateTime.Now,
                                    };

                                    var awaut = await UserManager.CreateAsync(user, "Admin");
                                }
              */
            }
        }
    }
}
