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
        public Tournament ReadTourData(string firstKey, string secondKey, string updateVersion)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
            {
                if (SecurityHelper.checkAdminKeys(firstKey, secondKey) || SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                {
                    return DatabaseHelper.GetTournamentData();
                }
            }

            return new Tournament();
        }

        [System.Web.Http.HttpGet]
        public IEnumerable<TourPlayer> ReadTopTourPlayers(string firstKey, string secondKey, string updateVersion)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
            {
                if (SecurityHelper.checkAdminKeys(firstKey, secondKey) || SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                {
                    List<TourPlayer> topTourPlayers = DatabaseHelper.GetTopTourPlayers();

                    return topTourPlayers;
                }
            }

            return new List<TourPlayer>();
        }

        [System.Web.Http.HttpGet]
        public TourPlayer ReadMyTourData(string firstKey, string secondKey, long id)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey))
            {
                if (SecurityHelper.checkPlayerKeys(firstKey, secondKey) || SecurityHelper.checkAdminKeys(firstKey, secondKey))
                {
                    return DatabaseHelper.GetTourPlayerById(id);
                }
            }

            return new TourPlayer();
        }

        [System.Web.Http.HttpGet]
        public string AddTourPlayer(string firstKey, string secondKey, string name)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref name))
            {
                if (SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                {
                    Tournament tournament = DatabaseHelper.GetTournamentData();

                    if (tournament.active)
                    {
                        string passkey = SecurityHelper.makeKey64();

                        long userId = DatabaseHelper.AddTourPlayer(passkey, name);

                        return "success" + "," + userId + "," + passkey;
                    }
                }
            }

            return "failure";
        }

        [System.Web.Http.HttpGet]
        public string EditTourPlayer(string firstKey, string secondKey, long id, string passkey, string name, int score)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref passkey) && checkParam(ref name) && id >= 0)
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.UpdateTourPlayerById(id, passkey, name, score);

                        return "success";
                    }
                }
            }
            catch (Exception) { }

            return "failure";
        }

        [System.Web.Http.HttpGet]
        public string DeleteTourPlayer(string firstKey, string secondKey, long tourPlayerId, string passkey)
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
                            DatabaseHelper.DeleteTourPlayer(tourPlayerId, passkey);

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
