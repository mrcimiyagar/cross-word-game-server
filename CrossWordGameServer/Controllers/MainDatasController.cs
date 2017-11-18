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
    public class MainDatasController : ApiController
    {
        [System.Web.Http.HttpGet]
        public string ReadMainData(string firstKey, string secondKey, string updateVersion)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        int gameLevelsCount = DatabaseHelper.GetGameLevelsCount();
                        int messagesCount = DatabaseHelper.GetMessagesCount();
                        int wordsCount = DatabaseHelper.GetWordsCount();
                        Tournament tournament = DatabaseHelper.GetTournamentData();
                        return gameLevelsCount + "," + tournament.active + "," + tournament.totalDays + ","
                            + tournament.leftDays + "," + tournament.playersCount + "," + messagesCount + ","
                            + wordsCount;
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }

            return "";
        }

        [System.Web.Http.HttpGet]
        public string EnableTournament(string firstKey, string secondKey, int totalDays)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey) && totalDays > 0)
            {
                if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                {
                    Tournament tournament = DatabaseHelper.GetTournamentData();

                    if (!tournament.active)
                    {
                        DatabaseHelper.StartTournament(totalDays);

                        return "success";
                    }
                }
            }

            return "error";
        }

        [System.Web.Http.HttpGet]
        public string DisableTournament(string firstKey, string secondKey)
        {
            if (checkParam(ref firstKey) && checkParam(ref secondKey))
            {
                if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                {
                    Tournament tournament = DatabaseHelper.GetTournamentData();

                    if (tournament.active)
                    {
                        DatabaseHelper.EndTournament();
                    }
                }
            }

            return "error";
        }

        private bool checkParam(ref string param)
        {
            return param != null;
        }
    }
}
