using DemoApi.Helper;
using DemoApi.Model;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace DemoApi.Controller
{
    [ExceptionHandler]
    public class DemoController : ApiController
    {
        private readonly List<CustomerList> _customerList = new List<CustomerList>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Route("Demo/CreateCustomer/")]
        public HttpResponseMessage CreateCustomer([FromBody] CustomerList customer) 
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();

            using (var db = new DemoApiContext())
            {
                var ifCustomer = db.CustomerLists.FirstOrDefault(i => i.Email == customer.Email);

                if (ifCustomer != null)
                {
                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.OK, "Oops! email already in use!", JsonMediaTypeFormatter.DefaultMediaType);
                }

                customer.CreatedDate = DateTime.Now;
                db.CustomerLists.Add(customer);
                db.SaveChanges();

                _customerList.Add(new CustomerList { Id = customer.Id, Name = customer.Name, Email = customer.Email, CreatedDate = customer.CreatedDate});
            }

            return ControllerContext.Request
                .CreateResponse(HttpStatusCode.Created, _customerList, JsonMediaTypeFormatter.DefaultMediaType);

        }

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
        [Route("Demo/FileDownload/")]
        public HttpResponseMessage FileDownload([FromBody] FileUpload fileDetails)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

                if (ValidateBearerToken(bearerToken))
                {
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    var filePath = HttpContext.Current.Server.MapPath("~/" + fileDetails.FileName ); 

                    if (!File.Exists(filePath))
                    {
                        //Throw 404 (Not Found) exception if File not found.
                        var error = $"File not found: {fileDetails.FileName}";
                        return ControllerContext.Request
                            .CreateResponse(HttpStatusCode.NotFound, new { error });
                    }

                    var bytes = File.ReadAllBytes(filePath);
                    response.Content = new ByteArrayContent(bytes);
                    response.Content.Headers.ContentLength = bytes.LongLength;
                    response.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment") {FileName = fileDetails.FileName };
                    response.Content.Headers.ContentType =
                        new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileDetails.FileName));

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
        [Route("Demo/UpdateCustomer/")]
        public HttpResponseMessage UpdateCustomer([FromBody] CustomerList customer)
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();

            using (var db = new DemoApiContext())
            {
                var result = db.CustomerLists.FirstOrDefault(b => b.Id == customer.Id);
                if (result != null)
                {
                    result.Email = customer.Email;
                    result.Name = customer.Name;
                    db.SaveChanges();

                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.NotFound, result, JsonMediaTypeFormatter.DefaultMediaType);
                }
                else
                {
                    const string error = "Oops! No matching customer ID found!";

                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.NotFound, new { error } , JsonMediaTypeFormatter.DefaultMediaType);
                }
            }

        }

        [Route("Demo/DeleteCustomer/")]
        public HttpResponseMessage DeleteCustomer([FromBody] CustomerList customer)
        {
            var httpRequest = HttpContext.Current.Request;
            var bearerToken = httpRequest.Headers["Authorization"].Split(' ')[1];

            if (!ValidateBearerToken(bearerToken)) return UnauthorizedRequest();

            using (var db = new DemoApiContext())
            {
                var result = db.CustomerLists.SingleOrDefault(b => b.Id == customer.Id);
                if (result != null)
                {
                    db.CustomerLists.Attach(result);
                    db.CustomerLists.Remove(result);
                    db.SaveChanges();

                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.OK, result, JsonMediaTypeFormatter.DefaultMediaType);
                }
                else
                {
                    const string error = "Oops! No matching customer ID found!";

                    return ControllerContext.Request
                        .CreateResponse(HttpStatusCode.NotFound, new { error }, JsonMediaTypeFormatter.DefaultMediaType);
                }
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