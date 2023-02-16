using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System;
using System.Collections.Generic;

namespace Daimler.Providence.Tests
{
    public class TestHelper
    {
        public static void AssertNotFoundRequest(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            contentResult.StatusCode.ShouldBe(404);
        }

        public static void AssertBadRequest(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            contentResult.StatusCode.ShouldBe(400);
        }

        public static void AssertOkRequest(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            contentResult.StatusCode.ShouldBe(200);
        }

        public static object AssertCreatedRequest(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            contentResult.StatusCode.ShouldBe(201);
            
            if (contentResult.Content != null && contentResult.Content.Length > 0)
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(contentResult.Content);
                obj.ShouldNotBeNull();
                return obj;
            }
            return null;
        }

        public static void AssertAcceptedRequest(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            contentResult.StatusCode.ShouldBe(202);
        }

        public static void AssertNoContentRequest(IActionResult result)
        {
            var contentResult = result as NoContentResult;
            contentResult.ShouldNotBeNull();
            
        }

        public static object AssertContentRequest(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(contentResult.Content);
            obj.ShouldNotBeNull();
            return obj;
        }

        public static object AssertContentRequest(IActionResult result, Type type)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(contentResult.Content, type);
            obj.ShouldNotBeNull();
            return obj;
        }

        public static object AssertContentRequestList(IActionResult result, int amount)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(contentResult.Content);
            obj.ShouldNotBeNull();
            obj.Count.ShouldBe(amount);
            return obj;
        }

        public static object AssertContentRequestList(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(contentResult.Content);
            obj.ShouldNotBeNull();
            return obj;
        }

        public static object AssertContentRequestType(IActionResult result, Type type)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(contentResult.Content, type);
            obj.ShouldNotBeNull();
            return obj;
        }

        public static string AssertContentRequestString(IActionResult result)
        {
            var contentResult = result as ContentResult;
            contentResult.ShouldNotBeNull();
            return contentResult.Content;
        }
    }
}
