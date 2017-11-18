using CrossWordGameServer.Helpers;
using CrossWordGameServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;

namespace CrossWordGameServer.Controllers
{
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will be applied to all actions in MyController, unless those actions override with their own decoration
    public class WordsController : ApiController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<Word> ReadWords(string firstKey, string secondKey, string updateVersion)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey) || SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        return DatabaseHelper.GetWords();
                    }
                }
            }
            catch (Exception) { }

            return new List<Word>();
        }
        
        [System.Web.Http.HttpGet]
        public IEnumerable<int> ReadWordIds(string firstKey, string secondKey, string updateVersion)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey) || SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        return DatabaseHelper.GetWordIds();
                    }
                }
            }
            catch (Exception) { }

            return new List<int>();
        }

        [System.Web.Http.HttpGet]
        public string AddWord(string firstKey, string secondKey, string word, string meaning)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref word) && checkParam(ref meaning))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.AddWord(word, meaning);

                        return "success";
                    }
                }
            }
            catch (Exception ex) {
                return JsonConvert.SerializeObject(ex);
            }

            return "failure";
        }

        [System.Web.Http.HttpGet]
        public string DeleteWord(string firstKey, string secondKey, int wordId)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.DeleteWord(wordId);

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
