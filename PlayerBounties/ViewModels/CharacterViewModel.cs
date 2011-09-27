using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.ViewModels
{
    public class CharacterViewModel
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific properties

		public Character Character
		{
			get;
			set;
		}

		public Faction Faction
		{
			get;
			set;
		}

		public SelectList FactionList
		{
			get;
			set;
		}

		public PlayerClass PlayerClass
		{
			get;
			set;
		}

		public SelectList PlayerClassList
		{
			get;
			set;
		}

		public Race Race
		{
			get;
			set;
		}

		public IEnumerable<SelectListItem> RaceList
		{
			get;
			set;
		}

		public Shard Shard
		{
			get;
			set;
		}

		public SelectList ShardList
		{
			get;
			set;
		}

		#endregion
	}
}
