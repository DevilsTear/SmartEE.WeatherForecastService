using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartEE.WeatherForecast.Common.Validators
{
    /// <summary>
    /// BinaryInputFormatter Class
    /// </summary>
    public class BinaryInputFormatter : InputFormatter
    {
        const string binaryContentType = "application/octet-stream";
        const int bufferLength = 16384;

        /// <summary>
        /// BinaryInputFormatter initialization
        /// </summary>
        public BinaryInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(binaryContentType));
        }

        /// <summary>
        /// İstek gövdesinden bir nesne okur
        /// </summary>
        /// <param name="context">İstek gövdesinin bir nesneye serileştirilmesi için giriş biçimlendiricisi tarafından kullanılan bir bağlam nesnesi</param>
        /// <returns>ReadAsync işlem sonucu geri göndürülür</returns>
        public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            using (MemoryStream ms = new MemoryStream(bufferLength))
            {
                await context.HttpContext.Request.Body.CopyToAsync(ms);
                object result = ms.ToArray();
                return await InputFormatterResult.SuccessAsync(result);
            }
        }

        /// <summary>
        /// InputFormatter öğesinin ilgili nesne tipine deserilize edilip edilemeyeceğine karar verir
        /// </summary>
        /// <param name="type">Deserilize edilecek nesne</param>
        /// <returns>Eğer deselize edilebiliyorsa true aksi durumda false değer geri döndürülür</returns>
        protected override bool CanReadType(Type type)
        {
            if (type == typeof(byte[]))
                return true;
            else
                return false;
        }
    }
}
