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
		private List<SelectListItem> _shards = new List<SelectListItem>();
		private List<SelectListItem> _races = new List<SelectListItem>();

		private Faction faction = new Faction();
		
		#endregion

		#region Type specific properties

		public Character Character
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a faction")]
		public string SelectedFaction
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a class")]
		public string SelectedPlayerClass
		{
			get;
			set;
		}

		public string SelectedRace
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a shard")]
		public string SelectedShard
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
				return _shards;
			}
		}

		#endregion

	}
}