// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;

namespace Microsoft.Extensions.Caching.StackExchangeRedis
{
    /// <summary>
    /// Configuration options for <see cref="RedisCache"/>.
    /// </summary>
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// Gets or sets a delegate to create the ConnectionMultiplexer instance.
        /// </summary>
        public Func<Task<IConnectionMultiplexer>> ConnectionMultiplexerFactory { get; set; }

        /// <summary>
        /// The Redis instance name.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// The Redis profiling session
        /// </summary>
        public Func<ProfilingSession> ProfilingSession { get; set; }

        private bool _cacheSetFireAndForget;
        /// <summary>
        /// Gets or sets whether or not cache should be set using the "fire and forget" command option.
        ///
        /// Setting this to true will improve performance, but if "something" goes wrong we won't know about it. 
        ///
        /// https://stackexchange.github.io/StackExchange.Redis/Basics.html#sync-vs-async-vs-fire-and-forget
        /// </summary>
        public bool CacheSetFireAndForget
        {
            get => _cacheSetFireAndForget;

            set
            {
                _cacheSetFireAndForget = value;

                RedisCommandFlags = CommandFlags.FireAndForget;
            }
        }

        internal CommandFlags RedisCommandFlags { get; private set; } = CommandFlags.None;

        RedisCacheOptions IOptions<RedisCacheOptions>.Value
        {
            get { return this; }
        }
    }
}
