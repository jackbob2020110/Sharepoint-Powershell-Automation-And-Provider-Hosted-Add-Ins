﻿using FeedbackTracker.Common;
using FeedbackTracker.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;

namespace DemoFeedbackTracker_2._0Web
{
    public class WebHooksController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value ";
        }

        /// <summary>
        /// Responds to requests generated by subscriptions registered with
        /// the SharePoint WebHook REST API. 
        /// </summary>
        /// <param name="validationToken">The validation token (guid) sent by SharePoint when
        /// validating the Notification URL for the web hook subscription.</param>
        public async Task<HttpResponseMessage> Post(string validationToken = null)
        {
            // If a validation token is present, we need to respond within 5 seconds by  
            // returning the given validation token. This only happens when a new 
            // web hook is being added
            if (validationToken != null)
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(validationToken);
                return response;
            }

            // Read and parse the request body.
            var content = await Request.Content.ReadAsStringAsync();
            var notifications = JsonConvert.DeserializeObject<ResponseModel<NotificationModel>>(content).Value;

            if (notifications.Count > 0)
            {
                // do something with the received notification
                ChangeManager changeManager = new ChangeManager();
                foreach (var notification in notifications)
                {
                    // Synchronous pattern: only for operations that always complete within 5 seconds!
                    //changeManager.ProcessNotification(notification);

                    // Async pattern: add to storage queue, that will trigger a webjob
                    string storageAccountConnString = WebConfigurationManager.ConnectionStrings["storageaccount"].ConnectionString;
                    changeManager.AddNotificationToQueue(storageAccountConnString, notification);
                }
            }

            // if we get here we assume the request was well received
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}