using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Collections.Generic;
using System;

namespace RestfullyDelicious.Models
{
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser
	{
		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
		{
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
			// Add custom user claims here
			return userIdentity;
		}
	}

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext()
			: base("DefaultConnection", throwIfV1Schema: false)
		{
			Database.SetInitializer(new ApplicationDbInitializer());
		}

		public DbSet<Bookmark> Bookmarks { get; set; }

		public DbSet<Tag> Tags { get; set; }

		public static ApplicationDbContext Create()
		{
			return new ApplicationDbContext();
		}
	}

	public class ApplicationDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
	{
		protected override void Seed(ApplicationDbContext context)
		{
			var user = context.Users.Add(new ApplicationUser() { Email = "a123@gmail.com", UserName = "myuser" });

			context.Tags.Add(new Tag() { Name = "Default Tag", UserId = user.Id });
			context.SaveChanges();

			List<Bookmark> defaultBookmarks = new List<Bookmark>();
			Tag defaultTag = Task.Run(async () => await context.Tags.FirstAsync()).Result;
			defaultBookmarks.Add(new Bookmark() { Description="Some cool Bookmark", Url= "http://google.com", Time=DateTime.Now, UserId=user.Id, Tags = new List<Tag>() { defaultTag } });
			defaultBookmarks.Add(new Bookmark() { Description="Another cool Bookmark", Url = "http://google.com", Time=DateTime.Now, UserId=user.Id, Tags = new List<Tag>() { defaultTag } });
			defaultBookmarks.Add(new Bookmark() { Description="Yet Another cool Bookmark", Url = "http://google.com", Time=DateTime.Now, UserId=user.Id, Tags = new List<Tag>() { defaultTag } });

			foreach (Bookmark bmk in defaultBookmarks)
				context.Bookmarks.Add(bmk);

			base.Seed(context);
		}
	}
}