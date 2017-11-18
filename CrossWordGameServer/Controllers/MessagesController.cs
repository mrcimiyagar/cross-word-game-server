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
    public class MessagesController : ApiController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<Message> ReadMessages(string firstKey, string secondKey, string updateVersion)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey) || SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        return DatabaseHelper.GetMessages();
                    }
                }
            }
            catch (Exception) { }

            return new List<Message>();
        }
        
        [System.Web.Http.HttpGet]
        public IEnumerable<int> ReadMessageIds(string firstKey, string secondKey, string updateVersion)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref updateVersion))
                {
                    if (SecurityHelper.checkPlayerKeys(firstKey, secondKey) || SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        return DatabaseHelper.GetMessageIds();
                    }
                }
            }
            catch (Exception) { }

            return new List<int>();
        }

        [System.Web.Http.HttpGet]
        public string AddMessage(string firstKey, string secondKey, string content)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey) && checkParam(ref content))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.AddMessage(content);

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
        public string DeleteMessage(string firstKey, string secondKey, int messageId)
        {
            try
            {
                if (checkParam(ref firstKey) && checkParam(ref secondKey))
                {
                    if (SecurityHelper.checkAdminKeys(firstKey, secondKey))
                    {
                        DatabaseHelper.DeleteMessage(messageId);

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
