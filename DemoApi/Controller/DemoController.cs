using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace DemoApi.Controller
{
    public class DemoController : ApiController
    {
        [HttpGet]
        [Route("Demo/GetCustomerList")]
        public HttpResponseMessage GetCustomerList()
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();
            var customer1 = new List<string>() {"ID : 0001", "Name : John  Doe", "Email : jhon@live.com"};
            var customer2 = new List<string>() {"Name : 0002", "Name : Jane Dan", "Email : jane@google.com"};

            return ControllerContext.Request
                .CreateResponse(HttpStatusCode.OK, new {customer1, customer2});

        }

        private HttpResponseMessage UnauthorizedRequest()
        {
            var unauthorizedRequest = new List<string>() { "Houston we have a problem!" , " Unauthorized request! :(" };
            return ControllerContext.Request
                .CreateResponse(HttpStatusCode.Unauthorized , new { unauthorizedRequest });
        }

        [HttpPost]
        [Route("Demo/PutFileUpload")]
        public HttpResponseMessage PutFileUpload()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

                if (ValidateBearerToken(bearerToken))
                {
                    var docFiles = new List<string>();

                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        var filePath = HttpContext.Current.Server.MapPath("~/" + postedFile.FileName);
                        postedFile.SaveAs(filePath);
                        docFiles.Add(filePath);
                    }

                    return Request.CreateResponse(HttpStatusCode.Created, docFiles);
                }

                else
                {
                    return UnauthorizedRequest();
                }
            }

            catch (Exception exp)
            {
                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.ExpectationFailed, new {exp});
            }
        }

        [HttpGet]
        [Route("Demo/GetFileDownload/{fileName}")]
        public HttpResponseMessage GetFileDownload(string fileName)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

                if (ValidateBearerToken(bearerToken))
                {
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    var filePath = HttpContext.Current.Server.MapPath("~/" + fileName + ".txt"); 

                    if (!File.Exists(filePath))
                    {
                        //Throw 404 (Not Found) exception if File not found.
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.ReasonPhrase = $"File not found: {fileName} .";
                        throw new HttpResponseException(response);
                    }

                    var bytes = File.ReadAllBytes(filePath);
                    response.Content = new ByteArrayContent(bytes);
                    response.Content.Headers.ContentLength = bytes.LongLength;
                    response.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment") {FileName = fileName};
                    response.Content.Headers.ContentType =
                        new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));

                    return response;
                }

                else
                {
                    return UnauthorizedRequest();
                }
            }

            catch (Exception exp)
            {
                return ControllerContext.Request
                    .CreateResponse(HttpStatusCode.ExpectationFailed, new {exp});
            }
        }

        private static bool ValidateBearerToken(string bearerToken)
        {
            var bearerTokenApp = ConfigurationManager.AppSettings["BearerToken"];

            return bearerTokenApp == bearerToken;
        }
    }
}