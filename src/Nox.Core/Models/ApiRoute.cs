﻿using System.Collections.ObjectModel;
using Nox.Core.Components;
using Nox.Core.Interfaces;
using Nox.Core.Interfaces.Api;

namespace Nox.Core.Models
{
    public sealed class ApiRoute : MetaBase, IApiRoute
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HttpVerb { get; set; } = "GET";

        ICollection<IApiRouteParameter>? IApiRoute.Parameters
        {
            get => Parameters?.ToList<IApiRouteParameter>();
            set => Parameters = value as ICollection<ApiRouteParameter>;
        }
        public ICollection<ApiRouteParameter>? Parameters { get; set; } = new Collection<ApiRouteParameter>();

        ICollection<IApiRouteResponse>? IApiRoute.Responses
        {
            get => Responses?.ToList<IApiRouteResponse>();
            set => Responses = value as ICollection<ApiRouteResponse>;
        }
        
        public ICollection<ApiRouteResponse>? Responses { get; set; } = new Collection<ApiRouteResponse>();
        
        public string TargetUrl { get; set; } = null!;
    }
}
