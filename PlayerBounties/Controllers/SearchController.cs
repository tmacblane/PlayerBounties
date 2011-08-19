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
		#region Type specific methods

		/// <summary>
		/// Searches for a character by the character name
		/// </summary>
		/// <param name="formCollection">uses the data entered in the txtSearch box on the search partial view</param>
		/// <returns>List of character data</returns>
		public ActionResult Index(FormCollection formCollection)
		{
			string characterName = formCollection["txtSearch"];
			Search search = new Search();

			List<Character> characters = search.FindAllCharactersByName(characterName);

			// SELECT * FROM [characters] WHERE name = '{0}' and server = '{1}', characterName, server
			// 
			// Return the list of results to the user with a "select" link that navigates the user to the 
			// create screen and populates the name/server/shard/allegiance/class
			//
			// If no results found, provide a button to create character now that navigates the user to the
			// create screen and populates the name/server
			return View(characters);
		}

		#endregion
	}
}