using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class PlayerClassRace
	{
		#region Fields

        private PlayerBountyContext db = new PlayerBountyContext();
        private PlayerClass playerClass = new PlayerClass();
        private Race race = new Race();

		#endregion

		#region Type specific properties

        [Key]
        public Guid Id
        {
            get;
            set;
        }

        public Guid PlayerClassId
        {
            get;
            set;
        }

        public Guid RaceId
        {
            get;
            set;
        }

        public IEnumerable<Race> Races
        {
            get
            {
                return race.GetRacesList();
            }
        }

        public IEnumerable<PlayerClass> PlayerClasses
        {
            get
            {
                return playerClass.GetPlayerClassesList();
            }
        }

		#endregion

        #region Type specific methods

        public Guid GetPlayerClassRaceId(Guid playerClassId, Guid raceId)
        {
            var playerClassRaceItem = this.db.PlayerClassRaces.Where(c => c.PlayerClassId == playerClassId).Where(r => r.RaceId == raceId);

            if (playerClassRaceItem.Count() > 0)
            {
                return playerClassRaceItem.Single().Id;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public bool IsRaceSelected(Guid playerClassId, Guid raceId)
        {
            var playerClassRaceId = this.db.PlayerClassRaces.Where(c => c.PlayerClassId == playerClassId).Where(r => r.RaceId == raceId);

            if (playerClassRaceId.Count() == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion
	}
}