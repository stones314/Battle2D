----------------------------------------
-- NOTE
--
-- After update: Move this file to
-- %AppData%/Roaming/Wireshark/plugins then resatart Wireshark
--
-- NOTE END
----------------------------------------

----------------------------------------
-- do not modify this table
local debug_level = {
    DISABLED = 0,
    LEVEL_1  = 1,
    LEVEL_2  = 2
}

-- set this DEBUG to debug_level.LEVEL_1 to enable printing debug_level info
-- set it to debug_level.LEVEL_2 to enable really verbose printing
-- note: this will be overridden by user's preference settings
local DEBUG = debug_level.LEVEL_1

local default_settings =
{
    debug_level  = DEBUG,
    port         = 50123,
    heur_enabled = false,
}

-- for testing purposes, we want to be able to pass in changes to the defaults
-- from the command line; because you can't set lua preferences from the command
-- line using the '-o' switch (the preferences don't exist until this script is
-- loaded, so the command line thinks they're invalid preferences being set)
-- so we pass them in as command arguments insetad, and handle it here:
local args={...} -- get passed-in args
if args and #args > 0 then
    for _, arg in ipairs(args) do
        local name, value = arg:match("(.+)=(.+)")
        if name and value then
            if tonumber(value) then
                value = tonumber(value)
            elseif value == "true" or value == "TRUE" then
                value = true
            elseif value == "false" or value == "FALSE" then
                value = false
            elseif value == "DISABLED" then
                value = debug_level.DISABLED
            elseif value == "LEVEL_1" then
                value = debug_level.LEVEL_1
            elseif value == "LEVEL_2" then
                value = debug_level.LEVEL_2
            else
                error("invalid commandline argument value")
            end
        else
            error("invalid commandline argument syntax")
        end

        default_settings[name] = value
    end
end

local dprint = function() end
local dprint2 = function() end
local function reset_debug_level()
    if default_settings.debug_level > debug_level.DISABLED then
        dprint = function(...)
            print(table.concat({"Lua:", ...}," "))
        end

        if default_settings.debug_level > debug_level.LEVEL_1 then
            dprint2 = dprint
        end
    end
end
-- call it now
reset_debug_level()

dprint2("Wireshark version = ", get_version())
dprint2("Lua version = ", _VERSION)

----------------------------------------
-- Unfortunately, the older Wireshark/Tshark versions have bugs, and part of the point
-- of this script is to test those bugs are now fixed.  So we need to check the version
-- end error out if it's too old.
local major, minor, micro = get_version():match("(%d+)%.(%d+)%.(%d+)")
if major and tonumber(major) <= 1 and ((tonumber(minor) <= 10) or (tonumber(minor) == 11 and tonumber(micro) < 3)) then
        error(  "Sorry, but your Wireshark/Tshark version ("..get_version()..") is too old for this script!\n"..
                "This script needs Wireshark/Tshark version 1.11.3 or higher.\n" )
end

-- more sanity checking
-- verify we have the ProtoExpert class in wireshark, as that's the newest thing this file uses
assert(ProtoExpert.new, "Wireshark does not have the ProtoExpert class, so it's too old - get the latest 1.11.3 or higher")

----------------------------------------


----------------------------------------
-- creates a Proto object, but doesn't register it yet
local b2d = Proto("b2d","B2D Protocol")

----------------------------------------
-- multiple ways to do the same thing: create a protocol field (but not register it yet)
-- the abbreviation should always have "<myproto>." before the specific abbreviation, to avoid collisions
local pf_unity_hdr   = ProtoField.new   ("Some bytes I guess Unity adds", "b2d.unity_hdr", ftypes.BYTES)
local pf_msg_id      = ProtoField.le_uint32("b2d.msg_id", "Msg ID")
local pf_data_len    = ProtoField.le_uint32("b2d.data_len", "Number of bytes of player data")
local pf_round       = ProtoField.le_uint32("b2d.round", "Round number for player")
local pf_data        = ProtoField.new   ("Binary Player Data", "b2d.data", ftypes.BYTES)


----------------------------------------
-- this actually registers the ProtoFields above, into our new Protocol
-- in a real script I wouldn't do it this way; I'd build a table of fields programmatically
-- and then set b2d.fields to it, so as to avoid forgetting a field
b2d.fields = {
    pf_unity_hdr,
    pf_msg_id,
    pf_round,
    pf_data_len,
    pf_data
}

--------------------------------------------------------------------------------
-- preferences handling stuff
--------------------------------------------------------------------------------

-- a "enum" table for our enum pref, as required by Pref.enum()
-- having the "index" number makes ZERO sense, and is completely illogical
-- but it's what the code has expected it to be for a long time. Ugh.
local debug_pref_enum = {
    { 1,  "Disabled", debug_level.DISABLED },
    { 2,  "Level 1",  debug_level.LEVEL_1  },
    { 3,  "Level 2",  debug_level.LEVEL_2  },
}

b2d.prefs.debug = Pref.enum("Debug", default_settings.debug_level,
                            "The debug printing level", debug_pref_enum)

b2d.prefs.port  = Pref.uint("Port number", default_settings.port,
                            "The UDP port number for b2d")

b2d.prefs.heur  = Pref.bool("Heuristic enabled", default_settings.heur_enabled,
                            "Whether heuristic dissection is enabled or not")

----------------------------------------
-- a function for handling prefs being changed
function b2d.prefs_changed()
    dprint2("prefs_changed called")

    default_settings.debug_level  = b2d.prefs.debug
    reset_debug_level()

    default_settings.heur_enabled = b2d.prefs.heur

    if default_settings.port ~= b2d.prefs.port then
        -- remove old one, if not 0
        if default_settings.port ~= 0 then
            dprint2("removing b2d from port",default_settings.port)
            DissectorTable.get("udp.port"):remove(default_settings.port, b2d)
        end
        -- set our new default
        default_settings.port = b2d.prefs.port
        -- add new one, if not 0
        if default_settings.port ~= 0 then
            dprint2("adding b2d to port",default_settings.port)
            DissectorTable.get("udp.port"):add(default_settings.port, b2d)
        end
    end

end

dprint2("b2d Prefs registered")


----------------------------------------
---- some constants for later use ----
-- the b2d header size
local b2d_HDR_LEN = 10

----------------------------------------
-- The following creates the callback function for the dissector.
-- It's the same as doing "b2d.dissector = function (tvbuf,pkt,root)"
-- The 'tvbuf' is a Tvb object, 'pktinfo' is a Pinfo object, and 'root' is a TreeItem object.
-- Whenever Wireshark dissects a packet that our Proto is hooked into, it will call
-- this function and pass it these arguments for the packet it's dissecting.
function b2d.dissector(tvbuf,pktinfo,root)
    dprint2("b2d.dissector called")

    -- set the protocol column to show our protocol name
    pktinfo.cols.protocol:set("b2d")

    -- We want to check that the packet size is rational during dissection, so let's get the length of the
    -- packet buffer (Tvb).
    -- Because b2d has no additional payload data other than itself, and it rides on UDP without padding,
    -- we can use tvb:len() or tvb:reported_len() here; but I prefer tvb:reported_length_remaining() as it's safer.
    local pktlen = tvbuf:reported_length_remaining()

    -- We start by adding our protocol to the dissection display tree.
    -- A call to tree:add() returns the child created, so we can add more "under" it using that return value.
    -- The second argument is how much of the buffer/packet this added tree item covers/represents - in this
    -- case (b2d protocol) that's the remainder of the packet.
    local tree = root:add(b2d, tvbuf:range(0,pktlen))

    -- now let's check it's not too short
    if pktlen < b2d_HDR_LEN then
        -- since we're going to add this protocol to a specific UDP port, we're going to
        -- assume packets in this port are our protocol, so the packet being too short is an error
        -- the old way: tree:add_expert_info(PI_MALFORMED, PI_ERROR, "packet too short")
        -- the correct way now:
        tree:add_proto_expert_info(ef_too_short)
        dprint("packet length",pktlen,"too short")
        return
    end

    local pos = 0
    tree:add(pf_unity_hdr, tvbuf:range(pos,10))
    pos = pos + 10

    if pktlen < b2d_HDR_LEN + 4 then
        -- Some unity internal message, ignore it
        return pos
    end
    
    -- Now let's add our transaction id under our b2d protocol tree we just created.
    -- The transaction id starts at offset 0, for 2 bytes length.
    tree:add_le(pf_msg_id, tvbuf:range(pos,4))

    -- We'd like to put the transaction id number in the GUI row for this packet, in its
    -- INFO column/cell.  First we need the transaction id value, though.  Since we just
    -- dissected it with the previous code line, we could now get it using a Field's
    -- FieldInfo extractor, but instead we'll get it directly from the TvbRange just
    -- to show how to do that.  We'll use Field/FieldInfo extractors later on...
    local msg_id = tvbuf:range(pos,4):le_uint()
    pos = pos + 4

    if msg_id == 1 then
        -- Save player msg

        -- first field is round no
        tree:add_le(pf_round, tvbuf:range(pos,4))
        local round = tvbuf:range(pos,4):le_uint()
        pos = pos + 4

        -- second field is data length
        tree:add_le(pf_data_len, tvbuf:range(pos,4))
        local data_len = tvbuf:range(pos,4):le_uint()
        pos = pos + 4

        -- last field is data
        tree:add(pf_data, tvbuf:range(pos,data_len))
        pos = pos + data_len

        -- info colum
        pktinfo.cols.info:set("Save Player Cmd for round ".. round ..", saving ".. data_len.."B of player data")
    end

    if msg_id == 2 then
        -- Save player reply
        -- only contains msg id
        -- info colum
        pktinfo.cols.info:set("Save Player Reply ")
    end

    if msg_id == 3 then
        -- Load player msg

        -- only field is round no
        tree:add_le(pf_round, tvbuf:range(pos,4))
        local round = tvbuf:range(pos,4):le_uint()
        pos = pos + 4

        -- info colum
        pktinfo.cols.info:set("Load Player Cmd for round ".. round .."")
    end

    if msg_id == 4 then
        -- Load player reply

        -- first field is data length
        tree:add_le(pf_data_len, tvbuf:range(pos,4))
        local data_len = tvbuf:range(pos,4):le_uint()
        pos = pos + 4

        -- last field is data
        tree:add(pf_data, tvbuf:range(pos,data_len))
        pos = pos + data_len

        -- info colum
        pktinfo.cols.info:set("Load Player Reply, ".. data_len .."B of player data returned")
    end

    dprint2("b2d.dissector returning",pos)

    -- tell wireshark how much of tvbuff we dissected
    return pos
end

----------------------------------------
-- we want to have our protocol dissection invoked for a specific UDP port,
-- so get the udp dissector table and add our protocol to it
DissectorTable.get("udp.port"):add(default_settings.port, b2d)
