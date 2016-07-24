using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace ReactNative.Modules.Image
{
    class DefaultUriLoader : IUriLoader
    {
        public async Task<IRandomAccessStreamWithContentType> OpenReadAsync(string uri)
        {
            var streamRef = RandomAccessStreamReference.CreateFromUri(new Uri(uri));
            return await streamRef.OpenReadAsync();
        }

        public async Task<IRandomAccessStream> OpenReadAsync(string uri, IDictionary<string, string> headers)
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await client.SendRequestAsync(request); 
            var buffer = await response.Content.ReadAsBufferAsync();
            return buffer.AsStream().AsRandomAccessStream();
        }

        public Task PrefetchAsync(string uri)
        {
            throw new NotImplementedException("Prefetch is not yet implemented.");
        }
    }
}
