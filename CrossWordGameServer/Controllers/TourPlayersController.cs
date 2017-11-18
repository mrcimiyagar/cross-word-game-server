using CrossWordGameServer.Helpers;
using CrossWordGameServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace CrossWordGameServer.Controllers
{
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will be applied to all actions in MyController, unless those actions override with their own decoration
    public class TourPlayersController : ApiController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<TourPlayer> ReadTopTourPlayers(string firstKey, string secondKey, string updateVersion)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
            {
                if (SecurityHelper.checkAdminKeys(firstKey, secondKey) || SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                {
                    return DatabaseHelper.GetTopTourPlayers();
                }
            }

            return new List<TourPlayer>();
        }

        [System.Web.Http.HttpGet]
        public string AddTourPlayer(string firstKey, string secondKey, string name)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref name))
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                    {
                        Tournament tournament = DatabaseHelper.GetTournamentData();

                        if (tournament.active)
                        {
                            string passkey = SecurityHelper.makeKey64();

                            DatabaseHelper.AddTourPlayer(passkey, name);

                            return "success";
                        }
                    }
                }
            }
            catch (Exception) { }

            return "failure";
        }

        [System.Web.Http.HttpGet]
        public string DeleteTourPlayer(string firstKey, string secondKey, int tourPlayerId, string passkey)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref passkey))
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                    {
                        Tournament tournament = DatabaseHelper.GetTournamentData();

                        if (tournament.active)
                        {
                            DatabaseHelper.DeleteTourPlayer(tourPlayerId);

                            return "success";
                        }
                    }
                }
            }
            catch (Exception) { }

            return "failure";
        }

        private bool checkParam(ref string param)
        {
            return param != null;
        }
    }
}
