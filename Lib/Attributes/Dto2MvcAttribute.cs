namespace Dto2Mvc.Lib.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class Dto2MvcAttribute : Attribute
    {
        public string Controller { get; }
        public string Action { get; }
        public HttpMethod Method { get; }

        public Dto2MvcAttribute(HttpMethod method, string controller, string action)
        {
            Controller = controller;
            Action = action;
        }

        public enum HttpMethod
        {
            Get,
            Post
        }
    }
}