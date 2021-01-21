using IPA.Loader;
using System.Text;
using UnityEngine;
using SiraUtil.Zenject;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DiTails
{
    public struct HttpResponse
    {
        public bool Successful { get; set; }
        public string? Content { get; set; }

        public HttpResponse(bool success, string? content)
        {
            Successful = success;
            Content = content;
        }
    }

    internal sealed class Http
    {
        internal Dictionary<string, string> PersistentRequestHeaders { get; private set; }

        internal Http(UBinder<Plugin, PluginMetadata> metadataBinder)
        {
            PersistentRequestHeaders = new Dictionary<string, string>();
            string userAgent = $"{metadataBinder.Value.Name}/{metadataBinder.Value.Version}";
            PersistentRequestHeaders.Add("User-Agent", userAgent);
        }

        internal async Task SendHttpAsyncRequest(UnityWebRequest request, CancellationToken? token = null)
        {
            foreach (var header in PersistentRequestHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            AsyncOperation asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    request.Abort();
                    return;
                }
                await Task.Delay(100);
            }
        }

        internal async Task<HttpResponse> GetAsync(string url, string? authBearerKey = null, CancellationToken? token = null)
        {
            using var request = UnityWebRequest.Get(url);
            if (authBearerKey != null)
            {
                request.SetRequestHeader("Authorization", $"Bearer {authBearerKey}");
            }
            request.timeout = 15;
            await SendHttpAsyncRequest(request, token);
            return new HttpResponse(!(request.isNetworkError || request.isHttpError || !request.isDone), Encoding.UTF8.GetString(request.downloadHandler.data));
        }

        internal async Task<HttpResponse> PostAsync(string url, string body, string? authBearerKey = null, CancellationToken? token = null)
        {
            using UnityWebRequest request = body == null ? UnityWebRequest.Post(url, body) : UnityWebRequest.Put(url, body);
            if (authBearerKey != null)
            {
                request.SetRequestHeader("Authorization", $"Bearer {authBearerKey}");
            }
            if (body != null)
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.method = "POST"; // WHAT THE FUCK, UNITY?!
            }
            request.timeout = 15;
            await SendHttpAsyncRequest(request, token);
            return new HttpResponse(!(request.isNetworkError || request.isHttpError || !request.isDone), Encoding.UTF8.GetString(request.downloadHandler.data));
        }
    }
}
