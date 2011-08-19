using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Search
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		public List<Character> FindAllCharactersByName(string characterName)
		{
			return this.db.Characters.Where(rows => rows.Name == characterName).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass).ToList();
		}

		#endregion
	}
}