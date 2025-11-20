//using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NorthStar4.Infrastructure
{
    public class FlowRequestReader
    {
        //public FlowRequest Create(NameValueCollection nameValueCollection)
        //{
        //    Dictionary<string, string> dictionary = nameValueCollection.Cast<string>()
        //        .Select(s => new { Key = s, Value = nameValueCollection[s] })
        //        .ToDictionary(p => p.Key, p => p.Value);

        //    return Create(dictionary);
        //}

        //public async Task<FlowRequest> ReadGetAsync(HttpRequest request)
        //{
        //    var queryString = request.QueryString.Value;
        //    HttpUtility.
        //    Dictionary<string, string> dictionary = request.QueryString.Value
        //        .ToDictionary(x => x.Key, x => x.Value);

        //    return Create(dictionary);
        //}

        //public async Task<FlowRequest> ReadPostAsync(FlowRequestContext context, IFileSystem fileSystem)
        //{
        //    var provider = new FlowTemporaryFileProvider(context, fileSystem);
        //    await context.HttpRequest.Body.ReadAsync(..Content.ReadAsMultipartAsync(provider);

        //    var flowRequest = Create(provider.FormData);
        //    flowRequest.TemporaryFile = provider.TemporaryFiles.Single();

        //    return flowRequest;
        //}

        //private FlowRequest Create(IDictionary<string, string> query)
        //{
        //    return new FlowRequest
        //    {
        //        FlowChunkNumber = Ulong(query, "flowChunkNumber"),
        //        FlowChunkSize = Ulong(query, "flowChunkSize"),
        //        FlowFilename = String(query, "flowFilename"),
        //        FlowIdentifier = String(query, "flowIdentifier"),
        //        FlowRelativePath = String(query, "flowRelativePath"),
        //        FlowTotalChunks = Ulong(query, "flowTotalChunks"),
        //        FlowTotalSize = Ulong(query, "flowTotalSize")
        //    };
        //}

        //private string String(IDictionary<string, string> values, string key, string defaultValue = null)
        //{
        //    string stringValue;

        //    if (values.TryGetValue(key, out stringValue))
        //    {
        //        return stringValue;
        //    }

        //    return defaultValue;
        //}

        //private ulong? Ulong(IDictionary<string, string> values, string key, ulong? defaultValue = null)
        //{
        //    string stringValue;

        //    if (values.TryGetValue(key, out stringValue))
        //    {
        //        ulong tempValue;
        //        if (ulong.TryParse(stringValue, out tempValue))
        //        {
        //            return tempValue;
        //        }
        //    }

        //    return defaultValue;
        //}
    }
}
