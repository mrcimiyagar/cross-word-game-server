using CrossWordGameServerProject.Helpers;
using CrossWordGameServerProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace CrossWordGameServerProject.Controllers
{
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will be applied to all actions in MyController, unless those actions override with their own decoration
    public class GameLevelsController : ApiController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<GameLevel> ReadGameLevels(string firstKey, string secondKey, string updateVersion)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        return DatabaseHelper.GetGameLevels();
                    }
                }
            }
            catch (Exception) { }

            return new List<GameLevel>();
        }

        [System.Web.Http.HttpGet]
        public string AddGameLevel(string firstKey, string secondKey, int number, int prize, string tableData, string questionData, string answerData)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref tableData) && checkParam(ref questionData) && checkParam(ref answerData))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.AddGameLevel(number, prize, tableData, questionData, answerData);

                        return "success";
                    }
                }
            }
            catch (Exception) { }

            return "failure";
        }

        [System.Web.Http.HttpGet]
        public string EditGameLevel(string firstKey, string secondKey, int gameLevelId, int number, int prize, string tableData, string questionData, string answerData)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref tableData) && checkParam(ref questionData) && checkParam(ref answerData))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.EditGameLevel(gameLevelId, number, prize, tableData, questionData, answerData);

                        return "success";
                    }
                }
            }
            catch (Exception) { }

            return "failure";
        }

        [System.Web.Http.HttpGet]
        public string DeleteGameLevel(string firstKey, string secondKey, int gameLevelId)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.DeleteGameLevel(gameLevelId);

                        return "success";
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
