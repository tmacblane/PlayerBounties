using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PlayerBounties.Models
{
	public class PlayerBountyInitializer : DropCreateDatabaseIfModelChanges<PlayerBountyContext>
	{
		protected override void Seed(PlayerBountyContext context)
		{
			var factions = new List<Faction>
			{
				new Faction { Name = "Galactic Republic" },
				new Faction { Name = "Sith Empire" }
			};

			factions.ForEach(faction => context.Factions.Add(faction));
			context.SaveChanges();

			var playerClasses = new List<PlayerClass>
			{
				new PlayerClass { Name = "Trooper" },
				new PlayerClass { Name = "Smuggler" },
				new PlayerClass { Name = "Jedi Knight" },
				new PlayerClass { Name = "Jedi Consular" },
				new PlayerClass { Name = "Bounty Hunter" },
				new PlayerClass { Name = "Imperial Agent" },
				new PlayerClass { Name = "Sith Warrior" },
				new PlayerClass { Name = "Sith Inquisitor" }
			};

			var races = new List<Race>
			{
				new Race { Name = "Twi'Lek"},
				new Race { Name = "Chiss"},
				new Race { Name = "Rattaki"},
				new Race { Name = "Human"},
				new Race { Name = "Zabrak"},
				new Race { Name = "Miraluka"},
				new Race { Name = "Mirialan"},
				new Race { Name = "Sith Pureblood"}
			};

			var shards = new List<Shard>
			{
				new Shard { Name = "Alderaan"},
				new Shard { Name = "Coruscant"},
				new Shard { Name = "Belsavis"},
				new Shard { Name = "Ilum"},
				new Shard { Name = "Ord Mantell"},
				new Shard { Name = "Hoth"},
				new Shard { Name = "Balmorra"},
				new Shard { Name = "Tython"},
				new Shard { Name = "Nar Shaddaa"},
				new Shard { Name = "Hutta"},
				new Shard { Name = "Dromund Kaas"},
				new Shard { Name = "Taris"},
				new Shard { Name = "Voss"},
				new Shard { Name = "Corellia"},
				new Shard { Name = "Korriban"},
				new Shard { Name = "Quesh"},
				new Shard { Name = "Tatooine"}
			};
		}
	}
}