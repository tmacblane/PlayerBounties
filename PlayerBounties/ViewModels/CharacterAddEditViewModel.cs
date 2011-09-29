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
	public class CharacterAddEditViewModel
	{
		#region Fields

		private List<SelectListItem> _playerClasses = new List<SelectListItem>();
		private List<SelectListItem> _races = new List<SelectListItem>();

		private Faction faction = new Faction();
		private Shard shard = new Shard();
		
		#endregion

		#region Type specific properties

		public Character Character
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a faction")]
		public Guid SelectedFaction
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a class")]
		public Guid SelectedPlayerClass
		{
			get;
			set;
		}

		public Guid? SelectedRace
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a shard")]
		public Guid SelectedShard
		{
			get;
			set;
		}

		public List<SelectListItem> Factions
		{
			get
			{
				return faction.GetFactionList();
			}
		}

		public List<SelectListItem> PlayerClasses
		{
			get
			{
				return _playerClasses;
			}
		}

		public List<SelectListItem> Races
		{
			get
			{
				return _races;
			}
		}

		public List<SelectListItem> Shards
		{
			get
			{
				return shard.GetShardList();
			}
		}

		#endregion

	}
}