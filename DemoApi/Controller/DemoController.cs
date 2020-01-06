using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace DemoApi.Controller
{
    public class DemoController : ApiController
    {
        [HttpGet]
        // GET api/demo/5
        public HttpResponseMessage GetCustomerList(int id)
        {
            var customer1 = new List<string>() { "ID : 0001", "Name : Jhon Doe", "Email : jhon@live.com" };
            var customer2 = new List<string>() { "Name : 0002", "Name : Jane Dan", "Email : jane@google.com" };

            return ControllerContext.Request
                .CreateResponse(HttpStatusCode.OK, new { customer1, customer2 });
        }

        //// POST api/demo
        //[HttpPost]
        //public HttpResponseMessage PostNewCustomer([FromBody]string value)
        //{
        //    var customer = new List<string>() { "ID : 0003", "Name : Robin Hood", "Email : robin@live.com" };

        //    return ControllerContext.Request
        //        .CreateResponse(HttpStatusCode.Created, new { customer });
        //}

        // POST api/demo
        [HttpPost]
        public HttpResponseMessage PutFileUpload()
        {
            try
            {
                HttpResponseMessage result = null;
                var httpRequest = HttpContext.Current.Request;
                var fileuploadPath = ConfigurationManager.AppSettings["FileUploadLocation"];

                {
                    var docfiles = new List<string>();
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        var filePath = HttpContext.Current.Server.MapPath("~/" + postedFile.FileName);
                        //var filePath = fileuploadPath + "\\" + postedFile.FileName;
                        postedFile.SaveAs(filePath);
                        docfiles.Add(filePath);
                    }
                    return result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
                }
            }

            catch (Exception exp)
            {
                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.ExpectationFailed ,new { exp });
            }
        }
        

        // DELETE api/demo/5
        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}