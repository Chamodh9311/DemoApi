using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using DemoApi.Model;

namespace DemoApi.Controller
{
    public class DemoController : ApiController
    {
        private readonly List<CustomerList> _customerList = new List<CustomerList>();

        [HttpGet]
        [Route("Demo/CreateCustomerList/{id = id}/{email = email}/{name = name}")]
        public HttpResponseMessage CreateCustomerList(int id, string email, string name)
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];
            
            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();

            _customerList.Add(new CustomerList{Id = id , Email = email , Name = name });

            return ControllerContext.Request
                .CreateResponse(HttpStatusCode.OK, new { _customerList });

        }

        [HttpPost]
        [Route("Demo/FileUpload")]
        public HttpResponseMessage FileUpload()
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

                    return Request.CreateResponse(HttpStatusCode.Created, "Path : " + docFiles.FirstOrDefault());
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
        [Route("Demo/FileDownload/{fileName = fileName}")]
        public HttpResponseMessage FileDownload(string fileName)
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
                        var error = $"File not found: {fileName}.txt";
                        return ControllerContext.Request
                            .CreateResponse(HttpStatusCode.NotFound, new { error });
                    }

                    var bytes = File.ReadAllBytes(filePath);
                    response.Content = new ByteArrayContent(bytes);
                    response.Content.Headers.ContentLength = bytes.LongLength;
                    response.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment") {FileName = fileName+".txt" };
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

        [HttpPut]
        [Route("Demo/UpdateCustomer/{id = id}/{email = email}")]
        public HttpResponseMessage UpdateCustomer(int id, string email)
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();

            _customerList.Add(new CustomerList { Id = 01, Email = "jhon@live.com", Name = "John Doe" });

            switch (id)
            {
                case 1:

                    var customer1 = _customerList.Select(n =>
                    {
                        if (n.Id == 1)
                        {
                            n.Email = email;
                        }

                        return n;
                    }).First();

                    return ControllerContext.Request
                         .CreateResponse(HttpStatusCode.Created, new { customer1 });

                default:
                    const string error = "Oops! No matching customer ID found!";

                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.NotFound, new { error });
            }
        }

        [HttpDelete]
        [Route("Demo/DeleteCustomer/{id = id}")]
        public HttpResponseMessage DeleteCustomer(int id)
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();

            switch (id)
            {
                case 1:

                    var response = "deleted Id : " + id;
                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.OK, new { response });

                default:
                    const string error = "Oops! No matching customer ID found!";

                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.NotFound, new { error });
            }
        }

        private static bool ValidateBearerToken(string bearerToken)
        {
            var bearerTokenApp = ConfigurationManager.AppSettings["BearerToken"];

            return bearerTokenApp == bearerToken;
        }

        private HttpResponseMessage UnauthorizedRequest()
        {
            var unauthorizedRequest = new List<string>() { "Houston we have a problem!", " Unauthorized request! :(" };
            return ControllerContext.Request
                .CreateResponse(HttpStatusCode.Unauthorized, new { unauthorizedRequest });
        }
    }
}