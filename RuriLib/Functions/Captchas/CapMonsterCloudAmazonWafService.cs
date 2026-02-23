using CaptchaSharp.Models;
using CaptchaSharp.Models.CaptchaResponses;
using CaptchaSharp.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace RuriLib.Functions.Captchas;

public class CapMonsterCloudAmazonWafService : CapMonsterCloudService
{
    public CapMonsterCloudAmazonWafService(string apiKey) : base(apiKey)
    {
    }

    public override async Task<StringResponse> SolveAmazonWafAsync(
        string siteKey, string iv, string context, string siteUrl,
        string? challengeScript = null, string? captchaScript = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
    {
        var requestBody = new JObject
        {
            { "clientKey", ApiKey },
            { 
                "task", new JObject
                {
                    { "type", "AmazonTaskProxyless" },
                    { "websiteURL", siteUrl },
                    { "websiteKey", siteKey },
                    { "iv", iv },
                    { "context", context }
                }
            }
        };

        if (!string.IsNullOrEmpty(challengeScript))
        {
            requestBody["task"]!["challengeScript"] = challengeScript;
        }

        if (!string.IsNullOrEmpty(captchaScript))
        {
            requestBody["task"]!["captchaScript"] = captchaScript;
        }

        var response = await HttpClient.PostAsync(
            "https://api.capmonster.cloud/createTask",
            new StringContent(requestBody.ToString(), System.Text.Encoding.UTF8, "application/json"),
            cancellationToken).ConfigureAwait(false);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var jsonResponse = JObject.Parse(responseContent);

        if (jsonResponse["errorId"]?.Value<int>() != 0)
        {
            throw new Exception($"CapMonsterCloud Error: {jsonResponse["errorCode"]} - {jsonResponse["errorDescription"]}");
        }

        var taskId = jsonResponse["taskId"]?.Value<string>();

        if (string.IsNullOrEmpty(taskId))
        {
            throw new Exception("CapMonsterCloud did not return a taskId.");
        }

        // Poll for result
        return await PollForResultAsync(taskId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<StringResponse> PollForResultAsync(string taskId, CancellationToken cancellationToken)
    {
        var requestBody = new JObject
        {
            { "clientKey", ApiKey },
            { "taskId", taskId }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(PollingInterval, cancellationToken).ConfigureAwait(false);

            var response = await HttpClient.PostAsync(
                "https://api.capmonster.cloud/getTaskResult",
                new StringContent(requestBody.ToString(), System.Text.Encoding.UTF8, "application/json"),
                cancellationToken).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var jsonResponse = JObject.Parse(responseContent);

            if (jsonResponse["errorId"]?.Value<int>() != 0)
            {
                throw new Exception($"CapMonsterCloud Error: {jsonResponse["errorCode"]} - {jsonResponse["errorDescription"]}");
            }

            var status = jsonResponse["status"]?.Value<string>();

            if (status == "ready")
            {
                var solution = jsonResponse["solution"];
                
                // For Amazon WAF, solution usually contains cookies or a token
                // We'll return the full solution JSON string as the response if it's not a simple string
                // But usually OpenBullet expects a single token if it's for a captcha block.
                // CapMonster returns an object with "captcha_voucher" and "existing_token" or "aws-waf-token"
                
                if (solution is JObject solutionObj)
                {
                    if (solutionObj.ContainsKey("captcha_voucher"))
                    {
                        return new StringResponse { Id = taskId, Response = solutionObj["captcha_voucher"]?.Value<string>() ?? "" };
                    }
                    
                    return new StringResponse { Id = taskId, Response = solutionObj.ToString() };
                }

                return new StringResponse { Id = taskId, Response = solution?.Value<string>() ?? "" };
            }
        }

        throw new TaskCanceledException();
    }
}
