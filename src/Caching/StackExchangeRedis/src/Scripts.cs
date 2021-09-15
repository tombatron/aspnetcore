// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.StackExchangeRedis
{
    internal static class Scripts
    {
        // KEYS[1] (@key)    = key
        // ARGV[1] (@absexp) = absolute-expiration - ticks as long (-1 for none)
        // ARGV[2] (@sldexp) = sliding-expiration - ticks as long (-1 for none)
        // ARGV[3] (@relexp) = relative-expiration (long, in seconds, -1 for none) - Min(absolute-expiration - Now, sliding-expiration)
        // ARGV[4] (@data)   = data - byte[]
        internal static LuaScript SetCache = LuaScript.Prepare(@"
            redis.call('HSET', @key, 'absexp', @absexp, 'sldexp', @sldexp, 'data', @data)
            if ARGV[3] ~= '-1' then
                redis.call('EXPIRE', @key, @relexp)
            end
            return 1");

        internal static LuaScript GetAndRefreshCache = LuaScript.Prepare(@"
            return getAndRefresh(@key, @getData)

            function getAndRefresh(key, getData)
                result = nil

                if getData == 1 then
                    result = redis.call('HGET', key, 'absexp', 'sldexp', 'data')
                else
                    result = redis.call('HGET', key, 'absexp', 'sldexp')
                end

                resultLen = tableLength(result)

                if resultLen >= 2 then
                    refresh(key, result)
                end

                if resultLen >= 3 and not result[3] == nil then
                    return result[3]
                end

                return nil
            end

            function refresh(key, result)
                absoluteExpiration = result[1]
                slidingExpiration = result[2]

                -- Refresh has no effect if there is just an absolute expiration(or neither).
                if not slidingExpiration == nil then
                    expiration = nil
                    slidingExpirationUnixTimestamp = ticksToUnixTimestamp(slidingExpiration)

                    if not absoluteExpiration == nil then
                        absoluteExpirationUnixTimestamp = ticksToUnixTimestamp(absoluteExpiration)
                        currentTimestamp = redis.call('TIME')

                        relativeExpiration = absoluteExpirationUnixTimestamp - currentTimestamp

                        if relativeExpiration <= slidingExpirationUnixTimestamp then
                            expiration = relativeExpiration
                        else
                            expiration = slidingExpirationUnixTimestamp;
                        end
                    else
                        expiration = slidingExpirationUnixTimestamp
                    end

                    redis.call('EXPIRE', key, expiration)
                end
            end

            function tableLength(tbl)
                len = 0

                for _ in pairs(tbl) do
                    len = len + 1
                end

                return len
            end

            function ticksToUnixTimestamp(ticks)
                unixTimestampBase = 62135596800

                ticksAsSeconds = ticks / 10000000

                return (ticksAsSeconds - unixTimestampBase)
            end

            function unixTimestampToTicks(unixTimestamp)
                unixTimestampBaseTicks = 621355968000000000

                unixTimestampAsTicks = unixTimestamp* 10000000

                return (unixTimestampBaseTicks + unixTimestampAsTicks)
            end");
    }
}
