using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
	public class SearchController : Controller
	{
		#region Fields

		private Search search = new Search();

		#endregion

		#region Type specific methods

		/// <summary>
		/// Searches for a character by the character name
		/// </summary>
		/// <param name="formCollection">uses the data entered in the txtSearch box on the search partial view</param>
		/// <returns>List of character data</returns>
		public ActionResult Index(FormCollection formCollection)
		{
			string characterName = formCollection["txtSearch"];

			List<Character> characters = this.search.FindAllCharactersByName(characterName);
			return View(characters);
		}

		#endregion
	}
}