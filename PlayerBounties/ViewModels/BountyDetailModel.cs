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
	public class BountyDetailModel
	{
		#region Fields

		private List<SelectListItem> _characters = new List<SelectListItem>();

		#endregion

		#region Type specific properties

		public Bounty Bounty
		{
			get;
			set;
		}

		public Character Character
		{
			get;
			set;
		}

		public Guid SelectedShard
		{
			get;
			set;
		}

		[Required(ErrorMessage = "You must upload an image")]
		[Display(Name = "File Name")]
		public Guid KillShotImageId
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Please select a character")]
		public Guid SelectedCharacter
		{
			get;
			set;
		}

		public List<SelectListItem> Characters
		{
			get
			{
				return _characters;
			}
		}

		#endregion
	}
}