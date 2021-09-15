// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.StackExchangeRedis
{
    internal static class RedisExtensions
    {
        internal static RedisValue[] HashMemberGet(this IDatabase cache, string key, params string[] members)
        {
            // TODO: Error checking?
            return cache.HashGet(key, GetRedisMembers(members));
        }

        internal static Task<RedisValue[]> HashMemberGetAsync(
            this IDatabase cache,
            string key,
            params string[] members)
        {
            // TODO: Error checking?
            return cache.HashGetAsync(key, GetRedisMembers(members));
        }

        private static RedisValue[] GetRedisMembers(string[] members)
        {
            var redisMembers = new RedisValue[members.Length];

            for (var i = 0; i < members.Length; i++)
            {
                redisMembers[i] = (RedisValue)members[i];
            }

            return redisMembers;
        }
    }
}
