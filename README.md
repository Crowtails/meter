# Crowtails: Crowfall dps meter
   WARNING: THERE IS NO HELP OR WARRANTY. THIS IS ENTIRELY AT YOUR OWN RISK AND YOU ASSUME ALL RESPONSIBILITY
   FOR USING THIS TOOL. IF YOU FEEL THIS PROVIDES GAMEPLAY ADVANTAGE OR AUTOMATION OR BREAKS ANY OTHER
   RULES DO NOT USE THIS.
   MIT license.

# how does it work
   Crowtails reads Crowfalls official log-files. All data collected is saved into a local database.
   If you join a group, those log-file-data will be transmittet to firebase by google and all group 
   members.
   The application does not send usage/user statistics of any kind.
   If you are a guild leader, you should consider to create a firebase account with realtime db to share dps data.
   
# Features
Realtime and archive statistics for crowfall:
 * Detail DMG/DMGin/Heal/Healin Skills->Target incl. crit and skill-tooltip
 * DMG type out
 * DMG type in
 * DMG out Graph
 * Heal out Graph
 * Heal in DMG in Graph
 * Total DMG out
 * Maxhit
 * Total Heal out
 * Max Heal
 * DMG Hit Count
 * Heal Hit count
 * Total Heal in
 * Total DMG in
 * MostdangerEnemy
 * Your Healer
 * Your Target
 * Dodges
 * Fight-Timer
 * DMG Crit Out 
 * Heal Crit Out 
 * DMG Crit In
 * heal hit in 
 * Heal Crit In 
 * group user 
 * RAID, full Logtransmition and raidfight detection incl. full group log
 * archiv statisics module.
 
# how do i install, update, or uninstall it
   Enable Crowfalls log function, by logging in the game > click settings > last settings page "write combat log".
   Use installer.
 
 # how do i use it
   don't be a dick.
   On first run, settings will apear first. You need to fill in accoutname first.
 
# about accuracy
   log data provided isnt exactly considered 100% accurate. 
   Some dmg types are still wrong, others are missing. A bunch of nice to have informations are nowhere to be found ...
   Crowtails uses a complete build in skill-list and enemy database to eliminate some of those problems.

# translations
   As of today only english / english client is supported. 

# Sourcecode
   Fast code, no documentation, no nice classes, no nice structure.

# limitations
   dmg types
   group dmg/heal without server
   pve inaccuracy

# it's not working
   make shure, crowfall write log options is on
   make sure the Dotnetframework 4.7 is installed.
   check if "windowskey + r" %appdata%/../locallow/Art+Craft/Crowfall/CombatLogs does contain log-files
   reinstall crowtails

# reporting crashes, bugs, and suggestions
   use bug-report option inside crowtails

# changes
   * Jun.11.2020: Release 1.0.0.0, source added.
   * Jun.09.2020: Network fixed.
   * Jun.07.2020: added Release Candidate.
