﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Script.Config;
using Microsoft.Azure.WebJobs.Script.Extensions;
using Microsoft.WebJobs.Script.Tests;
using Xunit;

namespace Microsoft.Azure.WebJobs.Script.Tests.Extensions
{
    public class HttpRequestExtensionsTest
    {
        [Fact]
        public void IsAppServiceInternalRequest_ReturnsExpectedResult()
        {
            // not running under Azure
            var request = HttpTestHelpers.CreateHttpRequest("GET", "http://foobar");
            Assert.False(request.IsAppServiceInternalRequest());

            // running under Azure
            var vars = new Dictionary<string, string>
            {
                { EnvironmentSettingNames.AzureWebsiteInstanceId, "123" }
            };
            using (var env = new TestScopedEnvironmentVariable(vars))
            {
                // with header
                var headers = new HeaderDictionary();
                headers.Add(ScriptConstants.AntaresLogIdHeaderName, "123");

                request = HttpTestHelpers.CreateHttpRequest("GET", "http://foobar", headers);
                Assert.False(request.IsAppServiceInternalRequest());

                request = HttpTestHelpers.CreateHttpRequest("GET", "http://foobar");
                Assert.True(request.IsAppServiceInternalRequest());
            }
        }

        [Fact]
        public void ConvertUserIdentitiesToString_RemovesCircularReference()
        {
            string expectedUserIdentities = "[{\"AuthenticationType\":\"TestAuthType\",\"IsAuthenticated\":true";
            IIdentity identity = new TestIdentity();
            Claim claim = new Claim("authlevel", "admin", "test", "LOCAL AUTHORITY", "LOCAL AUTHORITY");
            List<Claim> claims = new List<Claim>() { claim };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(identity, claims);
            List<ClaimsIdentity> claimsIdentities = new List<ClaimsIdentity>() { claimsIdentity };
            string userIdentitiesString = HttpRequestExtensions.GetUserIdentitiesAsString(claimsIdentities);
            Assert.Contains(expectedUserIdentities, userIdentitiesString);
        }
    }

    internal class TestIdentity : IIdentity
    {
        public string AuthenticationType => "TestAuthType";

        public bool IsAuthenticated => true;

        public string Name => "TestIdentityName";
    }
}
