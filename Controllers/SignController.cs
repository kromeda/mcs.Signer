using Newtonsoft.Json;
using Serilog;
using Signer.Filters;
using Signer.Models;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Signer.Controllers
{
    [RoutePrefix("sign")]
    [ExceptionHandler]
    public class SignController : ApiController
    {
        private readonly IFileSigner signer;

        public SignController(IFileSigner signer)
        {
            this.signer = signer;
        }

        [Route("detached/{serial}")]
        [HttpPost]
        public IHttpActionResult DetachedSign(string serial, [FromBody] byte[] file)
        {
            var detachedSign = signer.DetachedSignature(serial, file);

            if (detachedSign != null && detachedSign.Length > 0)
            {
                Log.Information("Файл подписан сертификатом с серийным номером {SerialNumber}", serial);
                return Ok(detachedSign);
            }
            else
            {
                var content = JsonConvert.SerializeObject(new
                {
                    Reason = "Не удалось получить открепленную подпись.",
                    Description = "Содержимое полученной подписи пусто."
                });

                Log.Warning("При попытке подписания файла подписью с серийным номером {SerialNumber} получен пустой результат.", serial);

                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json"),
                });
            }
        }

        [Route("signed/{serial}")]
        [HttpPost]
        public IHttpActionResult SignedFile(string serial, [FromBody] byte[] file)
        {
            var signedFile = signer.SignFile(serial, file);

            if (signedFile != null && signedFile.Length > 0)
            {
                Log.Information("Файл подписан сертификатом с серийным номером {SerialNumber}", serial);
                return Ok(signedFile);
            }
            else
            {
                var content = JsonConvert.SerializeObject(new
                {
                    Reason = "Не удалось подписать файл.",
                    Description = "Содержимое файла со встроенной подписью пусто."
                });

                Log.Warning("При попытке подписания файла подписью с серийным номером {SerialNumber} получен пустой результат.", serial);

                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json"),
                });
            }
        }
    }
}