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
            if @relexp ~= -1 then
                redis.call('EXPIRE', @key, @relexp)
            end
            return 1");

        internal static LuaScript GetAndRefreshCache = LuaScript.Prepare(@"
            local scope = {}

            function scope.getAndRefresh(key, getData)
                local result = nil

                if getData == '1' then
                    result = redis.call('HMGET', key, 'absexp', 'sldexp', 'data')
                else
                    result = redis.call('HMGET', key, 'absexp', 'sldexp')
                end

                if not scope.hasResult(result) then
                    return nil
                end

                local resultLen = scope.tableLength(result)

                if resultLen >= 2 then
                    scope.refresh(key, result)
                end

                if resultLen >= 3 and result[3] ~= nil then
                    return result[3]
                end

                return nil
            end

            function scope.refresh(key, result)
                local absoluteExpiration = result[1]
                local absoluteExpirationUnixTimestamp = nil

                if absoluteExpiration ~= '-1' then
                    absoluteExpirationUnixTimestamp = scope.ticksToUnixTimestamp(absoluteExpiration)
                end

                local slidingExpiration = result[2]
                local slidingExpirationUnixTimestamp = nil

                if slidingExpiration ~= '-1' then
                    slidingExpirationUnixTimestamp = slidingExpiration / 10000000
                end

                -- Refresh has no effect if there is just an absolute expiration(or neither).
                local expiration = nil
                local currentTime = tonumber(redis.call('TIME')[1])

                if slidingExpirationUnixTimestamp ~= nil then
                    if absoluteExpirationUnixTimestamp ~= nil then
                        local relExp = absoluteExpirationUnixTimestamp - currentTime

                        if relExp <= slidingExpirationUnixTimestamp then
                            expiration = relExp
                        else
                            expiration = slidingExpirationUnixTimestamp
                        end
                    else
                        expiration = slidingExpirationUnixTimestamp
                    end

                    redis.call('EXPIRE', key, expiration)
                end
            end

            function scope.tableLength(tbl)
                local len = 0

                for _ in pairs(tbl) do
                    len = len + 1
                end

                return len
            end

            function scope.hasResult(tbl)
                local result = false

                for i, v in ipairs(tbl) do
                    if v then
                        result = true
                        break
                    end
                end

                return result
            end

            function scope.ticksToUnixTimestamp(ticks)
                local unixTimestampBase = 62135596800

                local ticksAsSeconds = ticks / 10000000

                local result = (ticksAsSeconds - unixTimestampBase)

                return result
            end

            function scope.unixTimestampToTicks(unixTimestamp)
                local unixTimestampBaseTicks = 621355968000000000

                local unixTimestampAsTicks = unixTimestamp* 10000000

                return (unixTimestampBaseTicks + unixTimestampAsTicks)
            end

            return scope.getAndRefresh(@key, @getData)");
    }
}
