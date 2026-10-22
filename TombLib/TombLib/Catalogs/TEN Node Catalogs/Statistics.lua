local Statistics = require("Engine.RingInventory.Statistics")

-- !Name "Shows end of level statistics"
-- !Section "User interface"
-- !Description "Show end of level statistics."
-- !Arguments "NewLine, Numerical, 20, [ 0 | 99 ], Next level"
-- !Arguments "80, SoundTracks, Name of the audiotrack to play"
LevelFuncs.Engine.Node.ShowEndLevelStatistics = function(level, track)
    Statistics.SetType(false)
    TEN.Sound.PlayAudioTrack(track)
    Statistics.SetEndStatistics(true, level)
end

-- !Name "Shows end of game statistics"
-- !Section "User interface"
-- !Description "Show end of game statistics."
-- !Arguments "NewLine, 100, SoundTracks, Name of the audiotrack to play"
LevelFuncs.Engine.Node.ShowEndGameStatistics = function(track)
    Statistics.SetType(true)
    TEN.Sound.PlayAudioTrack(track)
    Statistics.SetEndStatistics(true, 999)
end
