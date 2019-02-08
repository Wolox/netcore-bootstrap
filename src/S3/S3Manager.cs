using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;

namespace NetCoreBootstrap.S3
{
    public class S3Manager
    {
        private readonly string _bucketName;
        private readonly IAmazonS3 _client;
        private PutObjectRequest putRequest;
        private GetObjectRequest getRequest;
        public S3Manager(IConfiguration configuration)
        {
            _client = new AmazonS3Client(
                configuration["AWS_ACCESS_KEY_ID"],
                configuration["AWS_SECRET_ACCESS_KEY"],
                RegionEndpoint.GetBySystemName(configuration["S3RegionName"]));
            _bucketName = configuration["S3BucketName"];
        }

        public async Task UploadAsync() => await _client.PutObjectAsync(putRequest);

        public async Task UploadAsync(string key, string contentBody)
        {
            if (putRequest == null)
            {
                putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    ContentBody = contentBody,
                };
            }
            else
            {
                putRequest.Key = key;
                putRequest.ContentBody = contentBody;
            }
            await _client.PutObjectAsync(putRequest);
        }

        public S3Manager BuildUploadRequest()
        {
            this.putRequest = new PutObjectRequest() { BucketName = _bucketName };
            return this;
        }

        public S3Manager WithContentDisposition(string contentDisposition)
        {
            this.putRequest.Headers.ContentDisposition = contentDisposition;
            return this;
        }

        public S3Manager WithContentEncoding(string contentEncoding)
        {
            this.putRequest.Headers.ContentEncoding = contentEncoding;
            return this;
        }

        public S3Manager WithContentCacheControl(string cacheControl)
        {
            this.putRequest.Headers.CacheControl = cacheControl;
            return this;
        }

        public S3Manager WithContentMD5(string contentMD5)
        {
            this.putRequest.Headers.ContentMD5 = contentMD5;
            return this;
        }

        public S3Manager WithContentLength(long contentLength)
        {
            this.putRequest.Headers.ContentLength = contentLength;
            return this;
        }

        public async Task GetAsync(string key) => await _client
                                                    .GetObjectAsync(new GetObjectRequest() { Key = key, BucketName = _bucketName });
    }
}
