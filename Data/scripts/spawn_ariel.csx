

using OpenTemple.Core.Systems.Script.Extensions;

var leader = GameSystems.Party.GetLeader();
var loc = leader.GetLocation().OffsetTiles(-1, 0);
var ariel = GameSystems.MapObject.CreateObject(14432, loc);
GameUiBridge.InitiateDialog(leader, ariel);
