﻿using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Nodexr.ApiShared;
using Nodexr.ApiShared.Pagination;
using Nodexr.Shared.NodeTreeBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nodexr.Shared.Services
{
    public class NodeTreeBrowserService
    {
        private readonly HttpClient httpClient;
        private readonly INodeHandler nodeHandler;
        private readonly string apiAddress;
        private NodeTreePreviewModel selectedNodeTree;

        public event EventHandler SelectedNodeTreeChanged;
        public NodeTreePreviewModel SelectedNodeTree
        {
            get => selectedNodeTree;
            set
            {
                selectedNodeTree = value;
                SelectedNodeTreeChanged(this, EventArgs.Empty);
            }
        }

        public NodeTreeBrowserService(HttpClient httpClient, INodeHandler nodeHandler, IConfiguration config)
        {
            this.httpClient = httpClient;
            this.nodeHandler = nodeHandler;
            this.apiAddress = config["apiAddress"];
        }

        public void LoadSelectedNodeTree()
        {
            if (SelectedNodeTree is null)
            {
                return;
            }
            nodeHandler.TryCreateTreeFromRegex(SelectedNodeTree.Expression);
            //TODO: Load search/replace strings
        }

        public async Task PublishNodeTree(NodeTreePublishModel model)
        {
            await httpClient.PostAsJsonAsync(
                $"{apiAddress}/nodetree",
                model
                ).ConfigureAwait(false);
        }

        public async Task<Paged<NodeTreePreviewModel>> GetAllNodeTrees(CancellationToken cancellationToken, string search = null, int start = 0, int limit = 50)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "start", start.ToString() },
                { "limit", limit.ToString() },
            };

            if (!string.IsNullOrEmpty(search))
            {
                queryParams.Add("search", search);
            }

            string uri = QueryHelpers.AddQueryString($"{apiAddress}/nodetree", queryParams);

            return await httpClient.GetFromJsonAsync<Paged<NodeTreePreviewModel>>(
                    uri,
                    cancellationToken
                    ).ConfigureAwait(false);
        }
    }
}